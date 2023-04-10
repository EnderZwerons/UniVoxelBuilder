using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this is a pretty lazy script but it might be not so lazy later on
public class MusicPlayer : MonoBehaviour
{
    public bool menuMusic;

    void Update()
    {
        if (!GetComponent<AudioSource>().isPlaying)
        {
            //sometimes gamedata.instance is null so
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
