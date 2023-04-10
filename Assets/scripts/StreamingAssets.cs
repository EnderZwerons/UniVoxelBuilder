using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this will of course get added onto as I add more things into streaming assets (I mean at this point unity is just the 3d rendering lol)
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
