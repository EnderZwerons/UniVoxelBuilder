using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPlacer : MonoBehaviour
{
    public GameObject currentBlockPrefab;

    public Transform instantiationPosition;

    public static BlockPlacer instance;

    public bool autoClicker;

    public void SetAutoClicker(bool active)
    {
        autoClicker = active;
    }

    public void SetBlockPrefab(GameObject newBlock)
    {
        currentBlockPrefab = newBlock;
        UIBlock.instance.ChangeOutBlock(newBlock);
    }

    public void SetBlockPrefabFromBlockIndex(int index)
    {
        SetBlockPrefab(GameData.instance.availableBlocks[index]);
    }

    public void PlaceBlock(BlockSide BS)
	{
		Vector3 targetPos = BS.block.transform.localPosition;
		Vector3 newPos = targetPos;
		switch (BS.blockSideType)
		{
			case BlockSide.BlockSideType.right:
			newPos.z += 1f;
			break;
			case BlockSide.BlockSideType.left:
			newPos.z -= 1f;
			break;
			case BlockSide.BlockSideType.down:
			newPos.y -= 1f;
			break;
			case BlockSide.BlockSideType.up:
			newPos.y += 1f;
			break;
			case BlockSide.BlockSideType.front:
			newPos.x += 1f;
			break;
			case BlockSide.BlockSideType.back:
			newPos.x -= 1f;
			break;
		}
		foreach (GameObject obj2 in GameObject.FindGameObjectsWithTag("block"))
		{
			if (obj2.transform.localPosition == newPos)
			{
				return;
			}
		}
        CreateBlock(newPos, currentBlockPrefab);
	}

    public void CreateBlock(Vector3 pos, GameObject blockPrefab)
    {
        GameObject block = Instantiate(blockPrefab, instantiationPosition);
        block.layer = 9;
		block.transform.localPosition = pos;
        block.name = block.name.Replace("(Clone)", "");
        GameData.instance.blockPlacedAmount++;
        GetComponent<AudioSource>().PlayOneShot(blockPrefab.GetComponent<Block>().place);
    }

    public void DestroyBlock(BlockSide BS)
    {
        Destroy(BS.block);
        GameData.instance.blockPlacedAmount--;
        GetComponent<AudioSource>().PlayOneShot(BS.block.GetComponent<Block>().destroy);
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentBlockPrefab = GameData.instance.planeBlock;
    }

    void Update()
    {
        if (UIController.inventoryOpen)
        {
            return;
        }
        if (autoClicker)
        {
            if (Input.GetMouseButton(1))
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 5f) && hit.transform.GetComponent<BlockSide>())
                {
                    if (Vector3.Distance(this.transform.position, hit.point) > 1.65f)
                    {
                        PlaceBlock(hit.transform.GetComponent<BlockSide>());
                        GetComponent<AudioSource>().PlayOneShot(currentBlockPrefab.GetComponent<Block>().place);
                    }
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(1))
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 5f) && hit.transform.GetComponent<BlockSide>())
                {
                    if (Vector3.Distance(this.transform.position, hit.point) > 1.65f)
                    {
                        PlaceBlock(hit.transform.GetComponent<BlockSide>());
                    }
                }
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit2;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit2, 5f) && hit2.transform.GetComponent<BlockSide>())
            {
                DestroyBlock(hit2.transform.GetComponent<BlockSide>());
            }
        }
        if (Input.GetMouseButtonDown(2))
        {
            RaycastHit hit3;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit3, 5f) && hit3.transform.GetComponent<BlockSide>())
            {
                SetBlockPrefab(GameData.instance.availableBlocks[hit3.transform.parent.gameObject.GetComponent<Block>().indexBlock]);
            }
        }
    }
}
