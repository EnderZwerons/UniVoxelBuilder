using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreamingAssets : MonoBehaviour
{
    public static string UVBMapPath
    {
        get
        {
            return Application.streamingAssetsPath + "/uvbmaps";
        }
    }

    public static string TexPath
    {
        get
        {
            return Application.streamingAssetsPath + "/tex";
        }
    }

    public static string SFXPath
    {
        get
        {
            return Application.streamingAssetsPath + "/sfx";
        }
    }

    public static string BlockListPath
    {
        get
        {
            return Application.streamingAssetsPath + "/blocks.blocklist";
        }
    }

    public static string BlockSoundsPath
    {
        get
        {
            return Application.streamingAssetsPath + "/blocksounds.audiolist";
        }
    }
}
