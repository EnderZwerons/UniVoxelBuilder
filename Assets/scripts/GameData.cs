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

    public BlockSounds[] blockSounds;

    public static BlockData.BlockSoundType GetBlockTypeFromString(string input)
    {
        switch (input)
        {
            case "stone":
                return BlockData.BlockSoundType.stone;
            case "grass":
                return BlockData.BlockSoundType.grass;
            case "snow":
                return BlockData.BlockSoundType.snow;
            case "wood":
                return BlockData.BlockSoundType.wood;
        }
        return BlockData.BlockSoundType.stone;
    }

    [Serializable]
    public class BlockData
    {
        public string name;

        public int index;

        public Texture tex;

        public BlockSoundType blockSoundType;

        public enum BlockSoundType
        {
            stone = 0,

            grass = 1,

            snow = 2,

            wood = 3
        };
    }

    [Serializable]
    public class BlockSounds
    {
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

    public static int BlocksAmount
    {
        get
        {
            int finalAmount = 0;
            List<BlockData> ParsedData = new List<BlockData>();
            string[] blockInfos = File.ReadAllText(StreamingAssets.BlockListPath).Split('\r', '\n');
            for (int i = 0; i < blockInfos.Length; i++)
            {
                if (blockInfos[i].StartsWith("//"))
                {
                    continue;
                }
                finalAmount++;
            }
            return finalAmount;
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
            block.indexBlock = blockData[i].index;
            block.place = blockSounds[(int)blockData[i].blockSoundType].place;
            block.destroy = blockSounds[(int)blockData[i].blockSoundType].destroy;
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
            if (blockInfos[i].StartsWith("//"))
            {
                continue;
            }
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
                    case "snd":
                    newBlockData.blockSoundType = GetBlockTypeFromString(blockLines[j + 1]);
                    break;
                }
                newBlockData.name = blockLines[0];
            }
            ParsedData.Add(newBlockData);
        }
        return ParsedData;
    }
}
