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

    public static string ModelPath
    {
        get
        {
            return Application.streamingAssetsPath + "/model";
        }
    }

    public static string GamedataPath
    {
        get
        {
            return Application.streamingAssetsPath + "/gamedata.nodelist";
        }
    }
}
