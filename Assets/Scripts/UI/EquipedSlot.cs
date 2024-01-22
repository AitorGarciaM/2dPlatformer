using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipedSlot : MonoBehaviour
{
	[SerializeField] private Image _image;
	[SerializeField] private int _id;

	private TrinketSlot _trinketSlot;

	public Trinket GetTrinket()
	{
		if (_trinketSlot != null)
		{
			return _trinketSlot.GetTrinket();
		}
		else
		{
			return null;
		}
	}
	public int Id { get { return _id; } }
	
	public void Equip(TrinketSlot trinketSlot)
	{
		if(trinketSlot.IsEquiped)
		{
			return;
		}

		if(_trinketSlot != null)
		{
			_trinketSlot.UnequipTrinket();
		}

		_trinketSlot = trinketSlot;
		_image.sprite = _trinketSlot.GetTrinket().GetSprite();
		_image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 1);
		_trinketSlot.EquipTrinket(this);

		PlayerController player = FindObjectOfType<PlayerController>();

		_trinketSlot.GetTrinket().GetEffect().Apply(player.gameObject);
	}

	public void Unequip()
	{
		_image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0);

		PlayerController player = FindObjectOfType<PlayerController>();
		_trinketSlot.GetTrinket().GetEffect().Disapply(player.gameObject);

		_trinketSlot.UnequipTrinket();
		_trinketSlot = null;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
}
