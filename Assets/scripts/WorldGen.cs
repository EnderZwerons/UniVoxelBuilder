using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

public class WorldGen : MonoBehaviour
{
    public GameObject[] blockPrefabs;

    public Transform instantiationPoint;

    public Vector2 startingPlane;

    public static WorldGen instance;

    public LayerMask blockLayer;

    public enum Biome
    {
        Hills = 0,

        Plains,

        Mountains
    };

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

    public IEnumerator GenerateBiomePerformance(int x, int z, int seed, Biome biomeType)
    {
        UnityEngine.Random.State originalRandomState = UnityEngine.Random.state;
        UnityEngine.Random.InitState(seed); // Initialize the random seed

        for (int i = 0; i < x; i++)
        {
            yield return null;
            float y = 0f;
            switch (biomeType)
            {
                case Biome.Hills:
                    y = Mathf.PerlinNoise((float)i / 10f, 0f) * 10f; // Use Perlin noise to generate Y-coordinate for Hills biome
                    break;
                case Biome.Plains:
                    y = Mathf.PerlinNoise((float)i / 10f, 0f) * 5f; // Use Perlin noise to generate Y-coordinate for Plains biome
                    break;
                case Biome.Mountains:
                    y = Mathf.PerlinNoise((float)i / 5f, 0f) * 20f; // Use Perlin noise to generate Y-coordinate for Mountains biome
                    break;
                default:
                    Debug.LogError("Invalid biome type!");
                    break;
            }
            int yInt = Mathf.RoundToInt(y); // Round the Y-coordinate to nearest integer
            CreateBlock(new Vector3(i, yInt, 0f), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);

            for (int i2 = 1; i2 < z; i2++)
            {
                float y2 = 0f;
                switch (biomeType)
                {
                    case Biome.Hills:
                        y2 = Mathf.PerlinNoise((float)i / 10f, (float)i2 / 10f) * 10f; // Use Perlin noise to generate Y-coordinate for Hills biome
                        break;
                    case Biome.Plains:
                        y2 = Mathf.PerlinNoise((float)i / 10f, (float)i2 / 10f) * 5f; // Use Perlin noise to generate Y-coordinate for Plains biome
                        break;
                    case Biome.Mountains:
                        y2 = Mathf.PerlinNoise((float)i / 5f, (float)i2 / 5f) * 20f; // Use Perlin noise to generate Y-coordinate for Mountains biome
                        break;
                    default:
                        Debug.LogError("Invalid biome type!");
                        break;
                }
                int yInt2 = Mathf.RoundToInt(y2);
                CreateBlock(new Vector3(i, yInt2, i2), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);
            }
        }
        UnityEngine.Random.state = originalRandomState;
    }


    public IEnumerator GenerateFlatPlanePreformancePerlinNoise(int x, int z, float multiplier)
    {
        for (int i = 0; i < x; i++)
        {
            yield return null;
            float perlin = (float)Math.Round(Mathf.PerlinNoise(UnityEngine.Random.Range(0, 0.99f), UnityEngine.Random.Range(0, 0.99f)) * multiplier);
            CreateBlock(new Vector3(i, perlin, 0f), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);
            if (perlin > 0f)
            {
                for (int j = (int)perlin; j > 0f; j--)
                {
                    CreateBlock(new Vector3(i, j, 0f), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);
                }
            }
            for (int i2 = 1; i2 < z; i2++)
            {
                float perlin2 = (float)Math.Round(Mathf.PerlinNoise(UnityEngine.Random.Range(0, 0.99f), UnityEngine.Random.Range(0, 0.99f)) * multiplier);
                CreateBlock(new Vector3(i, perlin2, i2), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);
                if (perlin2 > 0f)
                {
                    for (int j = (int)perlin2; j > 0f; j--)
                    {
                        CreateBlock(new Vector3(i, j, i2), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);
                    }
                }
            }
        }
    }

    public IEnumerator GenerateFromUVBMapPreformanceBinary(string fileName)
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
        DestroyAllBlocks();
        int pause = 0;
        List<UVBMAPLine> mapLines = UVBFormat.GetUVBMAPLineData((fileName.EndsWith(".uvbmap") ? StreamingAssets.UVBMapPath + "/" + fileName : StreamingAssets.UVBMapPath + "/" + fileName + ".uvbmap"));
        foreach (UVBMAPLine mapLine in mapLines)
        {
            pause++;
            if (pause > 32)
            {
                yield return null;
                pause = 0;
            }
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
        StartCoroutine(GenerateFlatPlanePreformance((int)startingPlane.x, (int)startingPlane.y));
    }

    public GameObject test;

    public void CreateBlock(Vector3 pos, GameObject blockPrefab)
    {
        GameObject block = Instantiate(blockPrefab, instantiationPoint);
        block.layer = 9;
        block.transform.localPosition = pos;
        block.name = block.name.Replace("(Clone)", "");
        GameData.instance.blockPlacedAmount++;
    }
}
