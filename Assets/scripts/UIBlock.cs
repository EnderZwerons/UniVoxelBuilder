using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBlock : MonoBehaviour
{
    public Transform blockInstantiationPoint;

    private GameObject currentBlock;

    public static UIBlock instance;

    void Awake()
    {
        instance = this;
    }

    public void ChangeOutBlock(GameObject newBlock)
    {
        if (currentBlock != null)
        {
            Destroy(currentBlock);
        }
        currentBlock = Instantiate(newBlock, blockInstantiationPoint);
        currentBlock.tag = "Untagged";
        currentBlock.transform.localPosition = Vector3.zero;
        currentBlock.layer = 8;
        Destroy(currentBlock.GetComponent<BoxCollider>());
        currentBlock.AddComponent<Rotator>().rotationAxes.y = 0.05f;
    }
}
