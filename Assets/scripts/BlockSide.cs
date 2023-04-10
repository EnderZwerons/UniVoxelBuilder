using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//lazyness at it's peak. I couldn't be bothered to find a real way to set up block colliders.
public class BlockSide : MonoBehaviour
{
	public enum BlockSideType
	{
		right = 0,

		left = 1,

		down = 2,

		up = 3,

		front = 4,

		back = 5
	};

	public BlockSideType blockSideType;

	public Block block
	{
		get
		{
			return transform.parent.gameObject.GetComponent<Block>();
		}
	}
}
