﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class WorldGen : MonoBehaviour
{
    public GameObject[] blockPrefabs;

    public Transform instantiationPoint;

    public Vector2 startingPlane;

    public static WorldGen instance;

    public LayerMask blockLayer;

    public void GenerateFlatPlane(int x, int z)
    {
        for (int i = 0; i < x; i++)
        {
            CreateBlock(new Vector3(i, 0f, 0f), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);

            for (int i2 = 1; i2 < z; i2++)
            {
                CreateBlock(new Vector3(i, 0f, i2), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);
            }
        }
    }

    public IEnumerator GenerateFlatPlanePreformance(int x, int z)
    {
        for (int i = 0; i < x; i++)
        {
            yield return null;

            CreateBlock(new Vector3(i, 0f, 0f), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);

            for (int i2 = 1; i2 < z; i2++)
            {
                CreateBlock(new Vector3(i, 0f, i2), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);
            }
        }
    }

    //unused for now but it actually works because chatgpt made it instead of me
    public IEnumerator GenerateBiomePerformance(int x, int z, int seed, GameData.Biome biome)
    {
        if (biome.perlinMult == 0)
        {
            StartCoroutine(GenerateFlatPlanePreformance(x, z));
            yield break;
        }
        UnityEngine.Random.State originalRandomState = UnityEngine.Random.state;
        UnityEngine.Random.InitState(seed); // Initialize the random seed

        for (int i = 0; i < x; i++)
        {
            yield return null;

            float y = 0f;
            y = Mathf.PerlinNoise((float)i / 10f, 0f) * biome.perlinMult;
            int yInt = Mathf.RoundToInt(y); // Round the Y-coordinate to nearest integer

            CreateBlock(new Vector3(i, yInt, 0f), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);

            for (int i2 = 1; i2 < z; i2++)
            {
                float y2 = 0f;
                y2 = Mathf.PerlinNoise((float)i / 10f, (float)i2 / 10f) * biome.perlinMult;
                int yInt2 = Mathf.RoundToInt(y2);

                CreateBlock(new Vector3(i, yInt2, i2), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);
            }
        }

        UnityEngine.Random.state = originalRandomState;
    }

    public IEnumerator GenerateFromUVBMapPreformanceBinary(string fileName)
    {
        string contents = "";

        //check if uvbmap file exists, if it doesn't tell the player it doesn't.
        try
        {
            contents = File.ReadAllText((fileName.EndsWith(".uvbmap") ? StreamingAssets.UVBMapPath + "/" + fileName : StreamingAssets.UVBMapPath + "/" + fileName + ".uvbmap"));
        }
        catch
        {
            UIController.instance.Popup("could not find uvbmap file!", 2f);
            yield break;
        }

        DestroyAllBlocks();
        int pause = 0;
        List<UVBMAPLine> mapLines = UVBFormat.GetUVBMAPLineData((fileName.EndsWith(".uvbmap") ? StreamingAssets.UVBMapPath + "/" + fileName : StreamingAssets.UVBMapPath + "/" + fileName + ".uvbmap"));

        foreach (UVBMAPLine mapLine in mapLines)
        {
            //pause every 32 blocks to generate fast, but to also keep it performance-friendly
            pause++;
            if (pause > 32)
            {
                yield return null;
                pause = 0;
            }

            //lazy buffer
            try
            {
                CreateBlock(new Vector3(mapLine.x, mapLine.y, mapLine.z), GameData.instance.availableBlocks[mapLine.index]);
            }
            catch (Exception e)
            {
                Debug.Log("could not create block! : " + e.ToString());
            }
        }

        UIController.instance.Popup("generated map from " + (fileName.EndsWith(".uvbmap") ? fileName : fileName + ".uvbmap"), 2f);
    }

    //just read the other comments I'm lazy
    public IEnumerator GenerateFromUVBMapPreformace(string fileName)
    {
        string contents = "";

        try
        {
            contents = File.ReadAllText((fileName.EndsWith(".uvbmap") ? StreamingAssets.UVBMapPath + "/" + fileName : StreamingAssets.UVBMapPath + "/" + fileName + ".uvbmap"));
        }
        catch
        {
            UIController.instance.Popup("could not find uvbmap file!", 2f);
            yield break;
        }

        //check if file is binary
        if (contents.Contains("!"))
        {
            StartCoroutine(GenerateFromUVBMapPreformanceBinary(fileName));
            yield break;
        }

        DestroyAllBlocks();
        int pause = 0;
        foreach (string line in contents.Split('\r', '\n'))
        {
            pause++;
            if (pause > 19)
            {
                yield return null;
                pause = 0;
            }

            string[] UVBParams = line.Split(',');

            try
            {
                CreateBlock(new Vector3(float.Parse(UVBParams[1]), float.Parse(UVBParams[2]), float.Parse(UVBParams[3])), GameData.instance.availableBlocks[int.Parse(UVBParams[0])]);
            }
            catch (Exception e)
            {
                Debug.Log("could not create block! : " + e.ToString());
            }
        }

        UIController.instance.Popup("generated map from " + (fileName.EndsWith(".uvbmap") ? fileName : fileName + ".uvbmap"), 2f);
    }

    //unused for now
    public static Mesh CombineBlocks()
    {
        List<MeshFilter> meshFilters = new List<MeshFilter>();
        foreach (GameObject block in GameObject.FindGameObjectsWithTag("block"))
        {
            meshFilters.Add(block.GetComponent<MeshFilter>());
        }
         CombineInstance[] combine = new CombineInstance[meshFilters.Count];
        for (int i = 0; i < combine.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);
        return mesh;
    }

    //unused
    public void GenerateFromUVBMap(string fileName)
    {
        string contents = File.ReadAllText((fileName.EndsWith(".uvbmap") ? StreamingAssets.UVBMapPath + "/" + fileName : StreamingAssets.UVBMapPath + "/" + fileName + ".uvbmap"));
        DestroyAllBlocks();
        foreach (string line in contents.Split('\r', '\n'))
        {
            string[] UVBParams = line.Split(',');
            CreateBlock(new Vector3(float.Parse(UVBParams[1]), float.Parse(UVBParams[2]), float.Parse(UVBParams[3])), GameData.instance.availableBlocks[int.Parse(UVBParams[0])]);
        }
        UIController.instance.Popup("generated map from " + (fileName.EndsWith(".uvbmap") ? fileName : fileName + ".uvbmap"), 2f);
    }

    public void DestroyAllBlocks()
    {
        foreach (GameObject block in GameObject.FindGameObjectsWithTag("block"))
        {
            if (block.layer == 9)
            {
                Destroy(block);
            }
        }
        UIController.instance.Popup("destroyed all blocks.", 1f);
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        blockPrefabs[0] = GameData.instance.planeBlock;
        startingPlane = GameData.instance.planeSize;
        StartCoroutine(GenerateBiomePerformance((int)startingPlane.x, (int)startingPlane.y, UnityEngine.Random.Range(0, int.MaxValue), GameData.instance.currentBiome));
    }

    public GameObject test;

    public void CreateBlock(Vector3 pos, GameObject blockPrefab)
    {
        //instantiate the block
        GameObject block = Instantiate(blockPrefab, instantiationPoint);

        //setup layer, position, and name
        block.layer = 9;
        block.transform.localPosition = pos;
        block.name = block.name.Replace("(Clone)", "");

        //increment the block placed amount
        GameData.instance.blockPlacedAmount++;
    }
}
