using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//more of a data storage class, pretty self explanatory.
public class Block : MonoBehaviour
{
    public AudioClip place;

    public AudioClip destroy;

    public int indexBlock;

    public GameData.BlockData.BlockType blockType;
}
