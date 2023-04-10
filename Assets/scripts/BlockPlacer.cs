using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this will probably be redone in the favor of better optimization
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
        currentBlockPrefab.layer = 9;

        UIBlock.instance.ChangeOutBlock(newBlock);
    }

    public void SetBlockPrefabFromBlockIndex(int index)
    {
        SetBlockPrefab(GameData.instance.availableBlocks[index]);
    }

    public void PlaceBlock(BlockSide BS)
	{
        //lazy way to make the water not clip
        float yModifier = (BS.block.blockType == GameData.BlockData.BlockType.water ? currentBlockPrefab.GetComponent<Block>().blockType == GameData.BlockData.BlockType.water ? 0.0001f : -0.0001f : currentBlockPrefab.GetComponent<Block>().blockType == GameData.BlockData.BlockType.water ? 0.0001f : 0f);

		Vector3 targetPos = BS.block.transform.localPosition;
		Vector3 newPos = targetPos;

        //lazy way to find out where the block should be placed
		switch (BS.blockSideType)
		{
			case BlockSide.BlockSideType.right:
			newPos.z += 1f;
			break;
			case BlockSide.BlockSideType.left:
			newPos.z -= 1f;
			break;
			case BlockSide.BlockSideType.down:
			newPos.y -= 1f + yModifier;
			break;
			case BlockSide.BlockSideType.up:
			newPos.y += 1f + yModifier;
			break;
			case BlockSide.BlockSideType.front:
			newPos.x += 1f;
			break;
			case BlockSide.BlockSideType.back:
			newPos.x -= 1f;
			break;
		}

        //lazy way to make sure nothing bad happens and to also destroy the optimization
		foreach (GameObject obj2 in GameObject.FindGameObjectsWithTag("block"))
		{
			if (obj2.transform.localPosition == newPos)
			{
				return;
			}
		}

        //create the block at the pos
        CreateBlock(newPos, currentBlockPrefab);
	}

    public void CreateBlock(Vector3 pos, GameObject blockPrefab)
    {
        //setup block prefab
        GameObject block = Instantiate(blockPrefab, instantiationPosition);
        block.layer = 9;
		block.transform.localPosition = pos;
        block.name = block.name.Replace("(Clone)", "");

        //increment block placed amount and play the place sound
        GameData.instance.blockPlacedAmount++;
        GetComponent<AudioSource>().PlayOneShot(blockPrefab.GetComponent<Block>().place);
    }

    public void DestroyBlock(BlockSide BS)
    {
        Destroy(BS.block.gameObject);

        //increment block placed amount and play break sound
        GameData.instance.blockPlacedAmount--;
        GetComponent<AudioSource>().PlayOneShot(BS.block.destroy);
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
        //return if the inventory is open
        if (UIController.inventoryOpen)
        {
            return;
        }

        //if autoclicker, place automatically
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
            else if (Input.GetMouseButton(0))
            {
                RaycastHit hit2;
    
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit2, 5f) && hit2.transform.GetComponent<BlockSide>())
                {
                    if (hit2.transform.parent.gameObject.layer != 10)
                    {
                        DestroyBlock(hit2.transform.GetComponent<BlockSide>());
                    }
                }
            }
        }

        //else place semi-automatically.
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
                if (hit2.transform.parent.gameObject.layer != 10)
                {
                    DestroyBlock(hit2.transform.GetComponent<BlockSide>());
                }
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
