using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public bool menuMusic;

    void Update()
    {
        if (!GetComponent<AudioSource>().isPlaying)
        {
            try
            {
                GameData.instance.RandomMusic(menuMusic);
            }
            catch
            {
                Debug.Log("sorry what");
            }
            GetComponent<AudioSource>().Play();
        }
    }
}
