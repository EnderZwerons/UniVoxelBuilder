using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;
using Nodelist;
using System.Linq;

public class GameData : MonoBehaviour
{
    public Vector2 planeSize;

    public GameObject planeBlock;

    public List<GameObject> availableBlocks = new List<GameObject>();

    public Vector2[] planeSizes;

    public int blockPlacedAmount;

    public static GameData instance;

    public Dropdown blockList;

    public Dropdown blockListMenu;

    private bool finishedParsedBlockSounds;

    public List<AudioClip> menuMusic;

    public List<AudioClip> ingameMusic;

    public List<Skybox> parsedSkyboxes;

    public AudioSource menuMusicSource;

    public AudioSource ingameMusicSource;

    public List<BlockSounds> parsedBlockSounds;

    public List<Material> parsedMaterials;

    public List<GameObject> parsedBlockPrefabs;

    public static float renderDistance
    {
        get
        {
            return PlayerPrefs.GetFloat("renderDistance");
        }
        set
        {
            PlayerPrefs.SetFloat("renderDistance", value);
        }
    }

    [Serializable]
    public class BlockData
    {
        public string name;

        public int index;

        public Texture tex;

        public BlockSounds blockSounds = new BlockSounds();

        public enum BlockType
        {
            normal = 0,

            water = 1
        };

        public BlockType blockType;

        public string matName;

        public BlockPrefab blockPrefab;
    }

    [Serializable]
    public class BlockSounds
    {
        public string name;

        public AudioClip place;

        public AudioClip destroy;
    }

    [Serializable]
    public class Skybox
    {
        public string name;

        public Texture up;

        public Texture down;

        public Texture front;

        public Texture back;

        public Texture left;

        public Texture right;
    }

    [Serializable]
    public class BlockPrefab
    {
        public string name;

        public bool colOn;

        public List<Vector3> centers = new List<Vector3>();

        public List<Vector3> sizes = new List<Vector3>();

        public Mesh mesh;
    }

    public void SetRenderDistance(float distance)
    {
        renderDistance = distance;
    }

    void Start()
    {
        NodelistReader.InitializeNodelist(File.ReadAllText(StreamingAssets.GamedataPath), "Gamedata");
        InitializeMaterial();
        InitializeBlocks();
        StartCoroutine(InitializeMusic());
        InitializeSkyboxes();
    }

    void Update()
    {
        if (instance != this)
        {
            instance = this;
        }
    }

    public static BlockData.BlockType GetBlockTypeFromString(string name)
    {
        switch (name)
        {
            case "water":
            return BlockData.BlockType.water;
        }
        return BlockData.BlockType.normal;
    }

    public void SetPlaneBlock(int num)
    {
        planeBlock = availableBlocks[num];
    }

    public void SetPlaneSize(int num)
    {
        planeSize = planeSizes[num];
    }

    public static void SaveToUVBMAPFile(string fileName)
    {
        if (!File.Exists((fileName.EndsWith(".uvbmap") ? StreamingAssets.UVBMapPath + "/" + fileName : StreamingAssets.UVBMapPath + "/" + fileName + ".uvbmap")))
        {
            File.Create((fileName.EndsWith(".uvbmap") ? StreamingAssets.UVBMapPath + "/" + fileName : StreamingAssets.UVBMapPath + "/" + fileName + ".uvbmap")).Close();
        }
        StringBuilder SB = new StringBuilder();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("block"))
        {
            if (obj.layer != 9)
            {
                continue;
            }
            Block block = obj.GetComponent<Block>();
            SB.Append(string.Format("{0},{1},{2},{3}{4}", block.indexBlock, block.transform.localPosition.x, block.transform.localPosition.y, block.transform.localPosition.z, "\n"));
        }
        File.WriteAllText((fileName.EndsWith(".uvbmap") ? StreamingAssets.UVBMapPath + "/" + fileName : StreamingAssets.UVBMapPath + "/" + fileName + ".uvbmap"), SB.ToString());
    }

    public static Texture2D GetStreamingTex(string name, int width = 2, int height = 2, FilterMode filterMode = FilterMode.Point)
    {
        byte[] x = File.ReadAllBytes(StreamingAssets.TexPath + "/" + name + ".png");
        Texture2D a = new Texture2D(width, height, TextureFormat.RGBA32, false);
        ImageConversion.LoadImage(a, x);
        a.filterMode = filterMode;
        return a;
    }

    public static AudioClip LoadStreamingAudioClip(string path)
	{
		AudioClip audioClip = null;
		using (UnityWebRequest audioClip2 = UnityWebRequestMultimedia.GetAudioClip(StreamingAssets.SFXPath + "/" + path + ".ogg", AudioType.OGGVORBIS))
		{
			audioClip2.SendWebRequest();
			try
			{
				while (!audioClip2.isDone)
				{
				}
				if (audioClip2.isNetworkError || audioClip2.isHttpError)
				{
					Debug.Log(audioClip2.error + path);
				}
				else
				{
					audioClip = DownloadHandlerAudioClip.GetContent(audioClip2);
				}
			}
			catch (Exception ex)
			{
				Debug.Log(ex.Message + ", " + ex.StackTrace);
			}
		}
        try
        {
            audioClip.name = path;
        }
        catch
        {
            Debug.Log("looks like we couldn't find " + path);
        }
		return audioClip;
	}

    public static int BlocksAmount
    {
        get
        {
            return NodelistReader.GetNodelistFile("Gamedata").GetNode("blocklist").lines.Count;
        }
    }

    public void SetSkybox(Skybox SB)
    {
        RenderSettings.skybox.SetTexture("_UpTex", SB.up);
        RenderSettings.skybox.SetTexture("_DownTex", SB.down);
        RenderSettings.skybox.SetTexture("_FrontTex", SB.front);
        RenderSettings.skybox.SetTexture("_BackTex", SB.back);
        RenderSettings.skybox.SetTexture("_LeftTex", SB.left);
        RenderSettings.skybox.SetTexture("_RightTex", SB.right);
    }

    public void InitializeSkyboxes()
    {
        RenderSettings.skybox = GetMaterialFromList("skybox");
        List<string> skyboxLines = NodelistReader.GetNodelistFile("Gamedata").GetNode("skyboxes").lines;
        for (int i = 0; i < skyboxLines.Count; i++)
        {
            if (skyboxLines[i] == "")
            {
                continue;
            }
            Skybox newSkybox = new Skybox();
            string[] nameDataSplit = skyboxLines[i].Split(':');
            newSkybox.name = nameDataSplit[0];
            string[] data = nameDataSplit[1].Split(',');
            newSkybox.up = GetStreamingTex("skybx/" + data[0]);
            newSkybox.down = GetStreamingTex("skybx/" + data[1]);
            newSkybox.front = GetStreamingTex("skybx/" + data[2]);
            newSkybox.back = GetStreamingTex("skybx/" + data[3]);
            newSkybox.left = GetStreamingTex("skybx/" + data[4]);
            newSkybox.right = GetStreamingTex("skybx/" + data[5]);
            parsedSkyboxes.Add(newSkybox);
        }
        SetSkybox(parsedSkyboxes[0]);
    }

    public IEnumerator InitializeMusic()
    {
        List<string> ingameMusics = NodelistReader.GetNodelistFile("Gamedata").GetNode("ingamemusic").lines;
        List<string> menuMusics = NodelistReader.GetNodelistFile("Gamedata").GetNode("menumusic").lines;
        for (int i = 0; i < ingameMusics.Count; i++)
        {
            yield return null;
            ingameMusic.Add(LoadStreamingAudioClip(ingameMusics[i]));
        }
        for (int i = 0; i < menuMusics.Count; i++)
        {
            yield return null;
            menuMusic.Add(LoadStreamingAudioClip(menuMusics[i]));
        }
        RandomMusic(true);
        RandomMusic(false);
    }

    public void RandomMusic(bool menu)
    {
        if (menu)
        {
            menuMusicSource.clip = menuMusic[UnityEngine.Random.Range(0, menuMusic.Count)];
            return;
        }
        ingameMusicSource.clip = ingameMusic[UnityEngine.Random.Range(0, ingameMusic.Count)];
    }
    

    public void InitializeBlocks()
    {
        ParseBlockPrefabs();
        ParseBlockSounds();
        List<BlockData> blockData = ParseBlockList();
        for (int i = 0; i < BlocksAmount; i++)
        {
            if (blockData[i].name == "")
            {
                continue;
            }
            GameObject tempBlock = Instantiate(GetBlockPrefabFromList((int)blockData[i].blockType));
            tempBlock.layer = 10;
            tempBlock.name = blockData[i].name;
            Block block = null;
            if (tempBlock.GetComponent<Block>() == null)
            {
                block = tempBlock.AddComponent<Block>();
            }
            else
            {
                block = tempBlock.GetComponent<Block>();
            }
            block.place = blockData[i].blockSounds.place;
            block.destroy = blockData[i].blockSounds.destroy;
            block.indexBlock = blockData[i].index;
            block.blockType = blockData[i].blockType;
            tempBlock.GetComponent<MeshRenderer>().material = GetMaterialFromList(blockData[i].matName);
            tempBlock.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", blockData[i].tex);
            availableBlocks.Add(tempBlock);
        }
        List<string> options = new List<string>();
        for (int i = 0; i < availableBlocks.Count; i++)
        {
            options.Add(availableBlocks[i].name);
        }
        blockList.AddOptions(options);
        blockListMenu.AddOptions(options);
        planeBlock = availableBlocks[0];
    }

    public void ParseBlockPrefabs()
    {
        List<BlockPrefab> parsedPrefabs = new List<BlockPrefab>();
        string[] blockPrefabInfos = NodelistReader.GetNodelistFile("Gamedata").GetNode("blockprefabs").lines.ToArray();
        parsedBlockPrefabs = new List<GameObject>();
        for (int i = 0; i < blockPrefabInfos.Length; i++)
        {
            BlockPrefab newBlockPrefab = new BlockPrefab();
            string[] paramSplit = blockPrefabInfos[i].Split(':');
            newBlockPrefab.name = paramSplit[0];
            newBlockPrefab.colOn = paramSplit[1].Split(' ')[1] == "on";
            string[] boundSplit = paramSplit[2].Split(';');
            for (int j = 0; j < boundSplit.Length; j++)
            {
                string[] floatSplit = boundSplit[j].Split(',');
                Vector3[] bounds = new Vector3[] { new Vector3(float.Parse(floatSplit[0]), float.Parse(floatSplit[1]), float.Parse(floatSplit[2])), new Vector3(float.Parse(floatSplit[3]), float.Parse(floatSplit[4]), float.Parse(floatSplit[5])) };
                newBlockPrefab.centers.Add(bounds[0]);
                newBlockPrefab.sizes.Add(bounds[1]);
            }
            newBlockPrefab.mesh = LoadOBJFile(paramSplit[3]);
            parsedBlockPrefabs.Add(BlockFactory(newBlockPrefab));
        }
    }

    public GameObject GetBlockPrefabFromList(int index)
    {
        if (parsedBlockPrefabs == null)
        {
            print("waiting...");
        }
        return parsedBlockPrefabs[index];
    }

    public void ParseBlockSounds()
    {
        List<BlockSounds> parsedSounds = new List<BlockSounds>();
        string[] blockSoundInfos = NodelistReader.GetNodelistFile("Gamedata").GetNode("soundsettings").lines.ToArray();
        for (int i = 0; i < blockSoundInfos.Length; i++)
        {
            BlockSounds newBlockSounds = new BlockSounds();
            string[] nameDataSplit = blockSoundInfos[i].Split(':');
            newBlockSounds.name = nameDataSplit[0];
            string[] data = nameDataSplit[1].Split(',');
            newBlockSounds.place = LoadStreamingAudioClip(data[0]);
            newBlockSounds.destroy = LoadStreamingAudioClip(data[1]);
            parsedBlockSounds.Add(newBlockSounds);
        }
    }

    public void InitializeMaterial()
    {
        string[] matInfos = NodelistReader.GetNodelistFile("Gamedata").GetNode("mats").lines.ToArray();
        List<List<string>> matLines = new List<List<string>>();
        for (int i = 0; i < matInfos.Length; i++)
        {
            List<string> lines = new List<string>();
            for (;;i++)
            {
                if (matInfos[i] == "}")
                {
                    break;
                }
                else
                {
                    lines.Add(matInfos[i]);
                }
            }
            matLines.Add(lines);
        }
        for (int i = 0; i < matLines.Count; i++)
        {
            parsedMaterials.Add(ProcessMaterialData(matLines[i].ToArray()));
        }
    }

    public Material GetMaterialFromList(string name)
    {
        while (parsedMaterials == null)
        {
            print("waiting...");
        }
        return parsedMaterials.Find(x => x.name == name);
    }

    public Material ProcessMaterialData(string[] lines)
    {
        Material mat = new Material(Shader.Find(lines[0].Split(':')[1]));
        mat.name = lines[0].Split(':')[0];
        bool breakOut = false;
        for (int i = 1; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("SetColor"))
            {
                string[] paramsSplit = lines[i].Split('"');
                string[] colors = paramsSplit[2].Split(',');
                mat.SetColor(paramsSplit[1], new Color(float.Parse(colors[0]), float.Parse(colors[1]), float.Parse(colors[2]), float.Parse(colors[3])));
            }
            else if (lines[i].StartsWith("SetTexture"))
            {
                string[] paramsSplit = lines[i].Split('"');
                mat.SetTexture(paramsSplit[1], GetStreamingTex(paramsSplit[2]));
            }
            else if (lines[i].StartsWith("SetFloat"))
            {
                string[] paramsSplit = lines[i].Split('"');
                mat.SetFloat(paramsSplit[1], float.Parse(paramsSplit[2]));
            }
        }
        return mat;
    }

    public BlockSounds GetBlockSoundsFromName(string name)
    {
        while (parsedBlockSounds == null)
        {
            print("waiting...");
        }
        return parsedBlockSounds.Find(x => x.name == name);
    }

    public GameObject BlockFactory(BlockPrefab input)
    {
        GameObject obj = new GameObject(input.name);
        obj.tag = "block";
        obj.layer = 10;
        obj.AddComponent<MeshFilter>().sharedMesh = input.mesh;
        obj.AddComponent<MeshRenderer>();
        GameObject up = new GameObject("up");
        up.layer = 9;
        up.AddComponent<BlockSide>().blockSideType = BlockSide.BlockSideType.up;
        up.transform.parent = obj.transform;
        up.AddComponent<BoxCollider>().center = input.centers[0];
        up.GetComponent<BoxCollider>().size = input.sizes[0];
        up.GetComponent<BoxCollider>().isTrigger = !input.colOn;
        GameObject down = new GameObject("down");
        down.AddComponent<BlockSide>().blockSideType = BlockSide.BlockSideType.down;
        down.transform.parent = obj.transform;
        down.AddComponent<BoxCollider>().center = input.centers[1];
        down.GetComponent<BoxCollider>().size = input.sizes[1];
        down.GetComponent<BoxCollider>().isTrigger = !input.colOn;
        GameObject front = new GameObject("front");
        front.AddComponent<BlockSide>().blockSideType = BlockSide.BlockSideType.front;
        front.transform.parent = obj.transform;
        front.AddComponent<BoxCollider>().center = input.centers[2];
        front.GetComponent<BoxCollider>().size = input.sizes[2];
        front.GetComponent<BoxCollider>().isTrigger = !input.colOn;
        GameObject back = new GameObject("back");
        back.AddComponent<BlockSide>().blockSideType = BlockSide.BlockSideType.back;
        back.transform.parent = obj.transform;
        back.AddComponent<BoxCollider>().center = input.centers[3];
        back.GetComponent<BoxCollider>().size = input.sizes[3];
        back.GetComponent<BoxCollider>().isTrigger = !input.colOn;
        GameObject left = new GameObject("left");
        left.AddComponent<BlockSide>().blockSideType = BlockSide.BlockSideType.left;
        left.transform.parent = obj.transform;
        left.AddComponent<BoxCollider>().center = input.centers[4];
        left.GetComponent<BoxCollider>().size = input.sizes[4];
        left.GetComponent<BoxCollider>().isTrigger = !input.colOn;
        GameObject right = new GameObject("right");
        right.AddComponent<BlockSide>().blockSideType = BlockSide.BlockSideType.right;
        right.transform.parent = obj.transform;
        right.AddComponent<BoxCollider>().center = input.centers[5];
        right.GetComponent<BoxCollider>().size = input.sizes[5];
        right.GetComponent<BoxCollider>().isTrigger = !input.colOn;
        return obj;
    }

    public List<BlockData> ParseBlockList()
    {
        List<BlockData> ParsedData = new List<BlockData>();
        string[] blockInfos = NodelistReader.GetNodelistFile("Gamedata").GetNode("blocklist").lines.ToArray();
        for (int i = 0; i < blockInfos.Length; i++)
        {
            if (blockInfos[i] == "")
            {
                i--;
                continue;
            }
            BlockData newBlockData = new BlockData();
            string[] nameSeparator = blockInfos[i].Split(':');
            string[] blockLines = nameSeparator[1].Split(',');
            newBlockData.tex = GetStreamingTex(blockLines[0]);
            newBlockData.blockSounds = GetBlockSoundsFromName(blockLines[1]);
            if (blockLines.Length > 2)
            {
                newBlockData.blockType = GetBlockTypeFromString(blockLines[2]);
            }
            if (blockLines.Length > 3)
            {
                newBlockData.matName = blockLines[3];
            }
            else
            {
                newBlockData.matName = "default";
            }
            newBlockData.index = i;
            newBlockData.name = nameSeparator[0];
            ParsedData.Add(newBlockData);
        }
        return ParsedData;
    }

	public static Mesh LoadOBJFile(string filePath)
	{
		string[] array = File.ReadAllLines(StreamingAssets.ModelPath + "/" + filePath + ".obj");
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		List<Vector2> list3 = new List<Vector2>();
		List<int> list4 = new List<int>();
		foreach (string text in array)
		{
			if (text.StartsWith("v "))
			{
				string[] array2 = text.Split(' ');
				list.Add(new Vector3(float.Parse(array2[1]), float.Parse(array2[2]), float.Parse(array2[3])));
			}
			else if (text.StartsWith("vn "))
			{
                string[] array3 = text.Split(' ');
				list2.Add(new Vector3(float.Parse(array3[1]), float.Parse(array3[2]), float.Parse(array3[3])));
			}
			else if (text.StartsWith("vt "))
			{
                string[] array4 = text.Split(' ');
				list3.Add(new Vector2(float.Parse(array4[1]), float.Parse(array4[2])));
			}
			else if (text.StartsWith("f "))
			{
                string[] array5 = text.Split(' ');
				list4.Add(int.Parse(array5[1].Split('/')[0]) - 1);
				list4.Add(int.Parse(array5[2].Split('/')[0]) - 1);
				list4.Add(int.Parse(array5[3].Split('/')[0]) - 1);
			}
		}
		return new Mesh
		{
			vertices = list.ToArray(),
			triangles = list4.ToArray(),
			uv = list3.ToArray(),
			normals = list2.ToArray()
		};
	}
}
