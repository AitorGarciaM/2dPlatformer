using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level Elements / ChestSO")]
public class ChestSO : LevelElementSO
{
	[SerializeField]
	private Treasure _treasure;

	public Treasure Treasure { get { return _treasure; } }
}
