using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;

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

    public GameObject templateBlock;

    private bool finishedParsedBlockSounds;

    public List<AudioClip> menuMusic;

    public List<AudioClip> ingameMusic;

    public AudioSource menuMusicSource;

    public AudioSource ingameMusicSource;

    [Serializable]
    public class BlockData
    {
        public string name;

        public int index;

        public Texture tex;

        public BlockSounds blockSounds = new BlockSounds();
    }

    [Serializable]
    public class BlockSounds
    {
        public string name;

        public AudioClip place;

        public AudioClip destroy;
    }

    void Start()
    {
        InitializeBlocks();
        StartCoroutine(InitializeMusic());
    }

    void Update()
    {
        if (instance != this)
        {
            instance = this;
        }
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

    public static Texture2D GetStreamingTex(string name, int width = 2, int height = 2)
    {
        byte[] x = File.ReadAllBytes(StreamingAssets.TexPath + "/" + name + ".png");
        Texture2D a = new Texture2D(width, height, TextureFormat.RGBA32, false);
        ImageConversion.LoadImage(a, x);
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
            return File.ReadAllText(StreamingAssets.BlockListPath).Split('\r', '\n').Length;
        }
    }

    public List<List<string>> ReadMusicListFile(string content)
    {
        List<string> ingameMusics = new List<string>();
        List<string> menuMusics = new List<string>();
        string[] lines = content.Split(Environment.NewLine.ToCharArray());
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line == "menu music:")
            {
                i++;
                for (;;i++)
                {
                    if (lines[i].StartsWith("mus:"))
                    {
                        menuMusics.Add(lines[i].Split(':')[1]);
                    }
                    else if (lines[i] == "end")
                    {
                        break;
                    }
                }
            }
            else if (line == "ingame music:")
            {
                i++;
                for (;;i++)
                {
                    if (lines[i].StartsWith("mus:"))
                    {
                        ingameMusics.Add(lines[i].Split(':')[1]);
                    }
                    else if (lines[i] == "end")
                    {
                        break;
                    }
                }
            }
        }
        return new List<List<string>> { menuMusics, ingameMusics };
    }

    public IEnumerator InitializeMusic()
    {
        List<List<string>> musicListFileContents = ReadMusicListFile(File.ReadAllText(StreamingAssets.MusicListPath));
        for (int i = 0; i < musicListFileContents[0].Count; i++)
        {
            yield return null;
            menuMusic.Add(LoadStreamingAudioClip(musicListFileContents[0][i]));
        }
        for (int i = 0; i < musicListFileContents[1].Count; i++)
        {
            yield return null;
            ingameMusic.Add(LoadStreamingAudioClip(musicListFileContents[1][i]));
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
        string content = File.ReadAllText(StreamingAssets.BlockListPath);
        List<BlockData> blockData = ParseBlockList(content);
        for (int i = 0; i < BlocksAmount; i++)
        {
            GameObject tempBlock = Instantiate(templateBlock);
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
            tempBlock.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", blockData[i].tex);
            if (tempBlock.name != "")
            {
                availableBlocks.Add(tempBlock);
            }
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

    public List<BlockData> ParseBlockList(string content)
    {
        List<BlockData> ParsedData = new List<BlockData>();
        string[] blockInfos = content.Split('\r', '\n');
        for (int i = 0; i < blockInfos.Length; i++)
        {
            BlockData newBlockData = new BlockData();
            string[] blockLines = blockInfos[i].Split(' ');
            for (int j = 0; j < blockLines.Length; j++)
            {
                switch (blockLines[j])
                {
                    case "idx":
                    newBlockData.index = int.Parse(blockLines[j + 1]);
                    break;
                    case "tex":
                    Texture tex = GetStreamingTex(blockLines[j + 1]);
                    tex.filterMode = FilterMode.Point;
                    newBlockData.tex = tex;
                    break;
                    case "plc":
                    newBlockData.blockSounds.place = LoadStreamingAudioClip(blockLines[j + 1]);
                    break;
                    case "des":
                    newBlockData.blockSounds.destroy = LoadStreamingAudioClip(blockLines[j + 1]);
                    break;
                }
                newBlockData.name = blockLines[0];
            }
            ParsedData.Add(newBlockData);
        }
        return ParsedData;
    }
}
