using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class Trinket : Treasure
{

	[SerializeField] private Effect _effect;

	private int _equipedSlotId = -1;
	bool _isTacken;

	public Effect GetEffect() { return _effect; }
	public int EquipedSlotId { get { return _equipedSlotId; } }
	public bool IsTacken { get { return _isTacken; } }

	public void Reset()
	{
		_isTacken = false;
		_equipedSlotId = -1;
	}

	public void TackeTrinket()
	{
		_isTacken = true;
	}

	public void Equip(EquipedSlot equipedSlot)
	{
		_equipedSlotId = equipedSlot.Id;
	}

	public void Unequip()
	{
		_equipedSlotId = -1;
	}
}
