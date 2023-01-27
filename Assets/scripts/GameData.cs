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

    public static List<BlockSounds> parsedBlockSounds = new List<BlockSounds>();

    public Vector2[] planeSizes;

    public int blockPlacedAmount;

    public static GameData instance;

    public Dropdown blockList;

    public Dropdown blockListMenu;

    public GameObject templateBlock;

    public BlockSounds[] blockSounds;

    private bool finishedParsedBlockSounds;

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

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        InitializeBlocks();
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
                Debug.Log("continued because we were looking for 9 but got " + obj.layer);
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
		return audioClip;
	}

    public static int BlocksAmount
    {
        get
        {
            return File.ReadAllText(StreamingAssets.BlockListPath).Split('\r', '\n').Length;
        }
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
