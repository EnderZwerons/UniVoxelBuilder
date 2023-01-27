using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	public GameObject block
	{
		get
		{
			return transform.parent.gameObject;
		}
	}
}
