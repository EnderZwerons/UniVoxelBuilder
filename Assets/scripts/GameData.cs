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
using System.Runtime.Serialization.Formatters.Binary;

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

    //I'd explain the classes but they kinda explain themselves so....

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

    //sorry, make that 3. also yes I'm commenting from top to bottom. cry.
    public void SetRenderDistance(float distance)
    {
        renderDistance = distance;
    }

    void Start()
    {
        //initialize the great file format called nodelist (get it because it's bad)
        NodelistReader.InitializeNodelist(File.ReadAllText(StreamingAssets.GamedataPath), "Gamedata");

        //initialize data
        InitializeMaterial();
        InitializeBlocks();
        StartCoroutine(InitializeMusic());
        InitializeSkyboxes();
    }

    void Update()
    {
        //💀
        if (instance != this)
        {
            instance = this;
        }
    }

    public static BlockData.BlockType GetBlockTypeFromString(string name)
    {
        //look ok there will be more later
        switch (name)
        {
            case "water":
            return BlockData.BlockType.water;
        }

        return BlockData.BlockType.normal;
    }

    //these two methods are never used
    public void SetPlaneBlock(int num)
    {
        planeBlock = availableBlocks[num];
    }

    public void SetPlaneSize(int num)
    {
        planeSize = planeSizes[num];
    }
    //and are completely useless

    public static void SaveToUVBMAPFile(string fileName)
    {
        //if the uvbmap doesn't exist, make it.
        if (!File.Exists((fileName.EndsWith(".uvbmap") ? StreamingAssets.UVBMapPath + "/" + fileName : StreamingAssets.UVBMapPath + "/" + fileName + ".uvbmap")))
        {
            File.Create((fileName.EndsWith(".uvbmap") ? StreamingAssets.UVBMapPath + "/" + fileName : StreamingAssets.UVBMapPath + "/" + fileName + ".uvbmap")).Close();
        }

        //yes I'm using a string builder haha! wait why am I using a string builder?????
        StringBuilder SB = new StringBuilder();
        List<UVBMAPLine> parsedLines = new List<UVBMAPLine>();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("block"))
        {
            //I should not use findgameobjectswith tag. I really shouldn't. oh well. (maybe I'll make it better later)
            if (obj.layer != 9)
            {
                continue;
            }

            //ok not as bad now
            UVBMAPLine data = new UVBMAPLine
            {
                x = (int)obj.transform.localPosition.x,
                y = (int)obj.transform.localPosition.y,
                z = (int)obj.transform.localPosition.z,
                index = obj.GetComponent<Block>().indexBlock
            };
            parsedLines.Add(data);
        }

        //write the file format to the file
        UVBFormat.SaveUVB(parsedLines, (fileName.EndsWith(".uvbmap") ? StreamingAssets.UVBMapPath + "/" + fileName : StreamingAssets.UVBMapPath + "/" + fileName + ".uvbmap"));
    }

    public static Texture2D GetStreamingTex(string name, int width = 2, int height = 2, FilterMode filterMode = FilterMode.Point)
    {
        //thank you NitrogenDioxide.
        byte[] x = File.ReadAllBytes(StreamingAssets.TexPath + "/" + name + ".png");
        Texture2D a = new Texture2D(width, height, TextureFormat.RGBA32, false);
        ImageConversion.LoadImage(a, x);
        a.filterMode = filterMode;
        return a;
    }

    public static AudioClip LoadStreamingAudioClip(string path)
	{
        //you can only use ogg. screw you..
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
            //returns the length of lines in the blocklist node
            return NodelistReader.GetNodelistFile("Gamedata").GetNode("blocklist").lines.Count;
        }
    }

    public void SetSkybox(Skybox SB)
    {
        //I collapsed it into one line because I thought it was funny.
        RenderSettings.skybox.SetTexture("_UpTex", SB.up); RenderSettings.skybox.SetTexture("_DownTex", SB.down); RenderSettings.skybox.SetTexture("_FrontTex", SB.front); RenderSettings.skybox.SetTexture("_BackTex", SB.back); RenderSettings.skybox.SetTexture("_LeftTex", SB.left); RenderSettings.skybox.SetTexture("_RightTex", SB.right);
    }

    public void InitializeSkyboxes()
    {
        RenderSettings.skybox = GetMaterialFromList("skybox");
        List<string> skyboxLines = NodelistReader.GetNodelistFile("Gamedata").GetNode("skyboxes").lines;

        for (int i = 0; i < skyboxLines.Count; i++)
        {
            //another duct tape fix. layzyzgzy
            if (skyboxLines[i] == "")
            {
                continue;
            }

            //setup simple skybox data
            Skybox newSkybox = new Skybox();
            string[] nameDataSplit = skyboxLines[i].Split(':');
            newSkybox.name = nameDataSplit[0];
            string[] data = nameDataSplit[1].Split(',');

            //have an anyeurism because I collapsed it into one line. lol.
            newSkybox.up = GetStreamingTex("skybx/" + data[0]); newSkybox.down = GetStreamingTex("skybx/" + data[1]); newSkybox.front = GetStreamingTex("skybx/" + data[2]); newSkybox.back = GetStreamingTex("skybx/" + data[3]); newSkybox.left = GetStreamingTex("skybx/" + data[4]); newSkybox.right = GetStreamingTex("skybx/" + data[5]);

            //add the spaghetti to the parsedSkybox storage
            parsedSkyboxes.Add(newSkybox);
        }

        //set the first spaghetti to the skybox with more spaghetti. wow I'm so good at coding why aren't I making operating systems??
        SetSkybox(parsedSkyboxes[0]);
    }

    public IEnumerator InitializeMusic()
    {
        //get nodes
        List<string> ingameMusics = NodelistReader.GetNodelistFile("Gamedata").GetNode("ingamemusic").lines;
        List<string> menuMusics = NodelistReader.GetNodelistFile("Gamedata").GetNode("menumusic").lines;

        //insert them into music storage
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

        //what?? I know I did this for a reason but I'm too lazy to find out why
        RandomMusic(true);
        RandomMusic(false);
    }

    public void RandomMusic(bool menu)
    {
        //yes there are random music clips. I just haven't added any yet. so uh go crazy.
        if (menu)
        {
            menuMusicSource.clip = menuMusic[UnityEngine.Random.Range(0, menuMusic.Count)];
            return;
        }
        ingameMusicSource.clip = ingameMusic[UnityEngine.Random.Range(0, ingameMusic.Count)];
    }
    

    public void InitializeBlocks()
    {
        //parse block data
        ParseBlockPrefabs();
        ParseBlockSounds();
        List<BlockData> blockData = ParseBlockList();

        for (int i = 0; i < BlocksAmount; i++)
        {
            //lazy duct tape fix
            if (blockData[i].name == "")
            {
                continue;
            }

            //make block gameobject
            GameObject tempBlock = Instantiate(GetBlockPrefabFromList((int)blockData[i].blockType));
            tempBlock.layer = 10;
            tempBlock.name = blockData[i].name;

            //what is this?? why did I do it like this?? I'm too lazy to find out 😎
            Block block = null;
            if (tempBlock.GetComponent<Block>() == null)
            {
                block = tempBlock.AddComponent<Block>();
            }
            else
            {
                block = tempBlock.GetComponent<Block>();
            }

            //insert blockdata into block class
            block.place = blockData[i].blockSounds.place;
            block.destroy = blockData[i].blockSounds.destroy;
            block.indexBlock = blockData[i].index;
            block.blockType = blockData[i].blockType;

            //setup materials
            tempBlock.GetComponent<MeshRenderer>().material = GetMaterialFromList(blockData[i].matName);
            tempBlock.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", blockData[i].tex);

            //add to the block storage
            availableBlocks.Add(tempBlock);
        }

        //parse options list
        List<string> options = new List<string>();
        for (int i = 0; i < availableBlocks.Count; i++)
        {
            options.Add(availableBlocks[i].name);
        }

        //insert options into ui lists
        blockList.AddOptions(options);
        blockListMenu.AddOptions(options);

        //declare default plane
        planeBlock = availableBlocks[0];
    }

    public void ParseBlockPrefabs()
    {
        List<BlockPrefab> parsedPrefabs = new List<BlockPrefab>();
        string[] blockPrefabInfos = NodelistReader.GetNodelistFile("Gamedata").GetNode("blockprefabs").lines.ToArray();
        parsedBlockPrefabs = new List<GameObject>();

        for (int i = 0; i < blockPrefabInfos.Length; i++)
        {
            //initialize block prefab with simple info
            BlockPrefab newBlockPrefab = new BlockPrefab();
            string[] paramSplit = blockPrefabInfos[i].Split(':');
            newBlockPrefab.name = paramSplit[0];
            newBlockPrefab.colOn = paramSplit[1].Split(' ')[1] == "on";
            string[] boundSplit = paramSplit[2].Split(';');

            for (int j = 0; j < boundSplit.Length; j++)
            {
                //insert bounds into blockprefab
                string[] floatSplit = boundSplit[j].Split(',');
                /*yes I like doing ridiculous one-liners. how'd you know?*/ Vector3[] bounds = new Vector3[] { new Vector3(float.Parse(floatSplit[0]), float.Parse(floatSplit[1]), float.Parse(floatSplit[2])), new Vector3(float.Parse(floatSplit[3]), float.Parse(floatSplit[4]), float.Parse(floatSplit[5])) };
                newBlockPrefab.centers.Add(bounds[0]);
                newBlockPrefab.sizes.Add(bounds[1]);
            }

            //load the mesh and storage it into the list of parsedblockprefabs using the terrible "BlockFactory" method.
            newBlockPrefab.mesh = LoadOBJFile(paramSplit[3]);
            parsedBlockPrefabs.Add(BlockFactory(newBlockPrefab));
        }
    }

    public GameObject GetBlockPrefabFromList(int index)
    {
        //buffer because like I've said many times before, I'm lazy.
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
            //initialize blocksounds
            BlockSounds newBlockSounds = new BlockSounds();
            string[] nameDataSplit = blockSoundInfos[i].Split(':');
            newBlockSounds.name = nameDataSplit[0];

            //insert audio data into blocksounds
            string[] data = nameDataSplit[1].Split(',');
            newBlockSounds.place = LoadStreamingAudioClip(data[0]);
            newBlockSounds.destroy = LoadStreamingAudioClip(data[1]);

            //add it to the stored block sounds
            parsedBlockSounds.Add(newBlockSounds);
        }
    }

    public void InitializeMaterial()
    {
        string[] matInfos = NodelistReader.GetNodelistFile("Gamedata").GetNode("mats").lines.ToArray();

        //yes that's a list list. cry.
        List<List<string>> matLines = new List<List<string>>();

        for (int i = 0; i < matInfos.Length; i++)
        {
            List<string> lines = new List<string>();
            for (;;i++)
            {
                //check for end
                if (matInfos[i] == "}")
                {
                    break;
                }
                else
                {
                    //add the line of text to the material info
                    lines.Add(matInfos[i]);
                }
            }
            //add to the parse queuer (is that a word??? queuer?? I'm too lazy to check)
            matLines.Add(lines);
        }

        for (int i = 0; i < matLines.Count; i++)
        {
            //parse all in queue and add to the storage of materials
            parsedMaterials.Add(ProcessMaterialData(matLines[i].ToArray()));
        }
    }

    public Material GetMaterialFromList(string name)
    {
        //buffer because I'm lazy
        while (parsedMaterials == null)
        {
            print("waiting...");
        }
        return parsedMaterials.Find(x => x.name == name);
    }

    public Material ProcessMaterialData(string[] lines)
    {
        //create shader with shaderName
        Material mat = new Material(Shader.Find(lines[0].Split(':')[1]));
        mat.name = lines[0].Split(':')[0];

        for (int i = 1; i < lines.Length; i++)
        {
            //there will probably be more functions here but for now they're not needed

            //check for color set function
            if (lines[i].StartsWith("SetColor"))
            {
                string[] paramsSplit = lines[i].Split('"');
                string[] colors = paramsSplit[2].Split(',');
                mat.SetColor(paramsSplit[1], new Color(float.Parse(colors[0]), float.Parse(colors[1]), float.Parse(colors[2]), float.Parse(colors[3])));
            }

            //check for texture set function
            else if (lines[i].StartsWith("SetTexture"))
            {
                string[] paramsSplit = lines[i].Split('"');
                mat.SetTexture(paramsSplit[1], GetStreamingTex(paramsSplit[2]));
            }

            //check for float set function
            else if (lines[i].StartsWith("SetFloat"))
            {
                string[] paramsSplit = lines[i].Split('"');
                mat.SetFloat(paramsSplit[1], float.Parse(paramsSplit[2]));
            }

            //guys does this mean I made an interpreter? what a great language. a whole 3 functions..
        }

        return mat;
    }

    public BlockSounds GetBlockSoundsFromName(string name)
    {
        //buffer because I'm LAZY!!
        while (parsedBlockSounds == null)
        {
            print("waiting...");
        }
        return parsedBlockSounds.Find(x => x.name == name);
    }

    public GameObject BlockFactory(BlockPrefab input)
    {
        //this script is bad and lazy but screw you

        //create block
        GameObject obj = new GameObject(input.name);
        obj.tag = "block";
        obj.layer = 10;
        obj.AddComponent<MeshFilter>().sharedMesh = input.mesh;
        obj.AddComponent<MeshRenderer>();

        //create up collider
        GameObject up = new GameObject("up");
        up.layer = 9;
        up.AddComponent<BlockSide>().blockSideType = BlockSide.BlockSideType.up;
        up.transform.parent = obj.transform;
        up.AddComponent<BoxCollider>().center = input.centers[0];
        up.GetComponent<BoxCollider>().size = input.sizes[0];
        up.GetComponent<BoxCollider>().isTrigger = !input.colOn;

        //create down collider
        GameObject down = new GameObject("down");
        down.AddComponent<BlockSide>().blockSideType = BlockSide.BlockSideType.down;
        down.transform.parent = obj.transform;
        down.AddComponent<BoxCollider>().center = input.centers[1];
        down.GetComponent<BoxCollider>().size = input.sizes[1];
        down.GetComponent<BoxCollider>().isTrigger = !input.colOn;

        //create front collider
        GameObject front = new GameObject("front");
        front.AddComponent<BlockSide>().blockSideType = BlockSide.BlockSideType.front;
        front.transform.parent = obj.transform;
        front.AddComponent<BoxCollider>().center = input.centers[2];
        front.GetComponent<BoxCollider>().size = input.sizes[2];
        front.GetComponent<BoxCollider>().isTrigger = !input.colOn;

        //create side collider
        GameObject back = new GameObject("back");
        back.AddComponent<BlockSide>().blockSideType = BlockSide.BlockSideType.back;
        back.transform.parent = obj.transform;
        back.AddComponent<BoxCollider>().center = input.centers[3];
        back.GetComponent<BoxCollider>().size = input.sizes[3];
        back.GetComponent<BoxCollider>().isTrigger = !input.colOn;

        //create left collider
        GameObject left = new GameObject("left");
        left.AddComponent<BlockSide>().blockSideType = BlockSide.BlockSideType.left;
        left.transform.parent = obj.transform;
        left.AddComponent<BoxCollider>().center = input.centers[4];
        left.GetComponent<BoxCollider>().size = input.sizes[4];
        left.GetComponent<BoxCollider>().isTrigger = !input.colOn;

        //create right collider
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
            //duct tape fix. probably should figure out why this happens.
            if (blockInfos[i] == "")
            {
                i--;
                continue;
            }

            //insert basic block data
            BlockData newBlockData = new BlockData();
            string[] nameSeparator = blockInfos[i].Split(':');
            string[] blockLines = nameSeparator[1].Split(',');
            newBlockData.tex = GetStreamingTex(blockLines[0]);
            newBlockData.blockSounds = GetBlockSoundsFromName(blockLines[1]);

            //insert special block data
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

            //finish parsing index and name and add to the returned list
            newBlockData.index = i;
            newBlockData.name = nameSeparator[0];
            ParsedData.Add(newBlockData);
        }

        return ParsedData;
    }

	public static Mesh LoadOBJFile(string filePath)
	{
        //thanks chatgpt (I'm not lazy you're lazy)
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
