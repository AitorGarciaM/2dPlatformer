using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TrinketSlot : MonoBehaviour
{
	[SerializeField] private Trinket _trinket;
	[SerializeField] private Image _image;
	[SerializeField] private TextMeshProUGUI _trinketName;
	[SerializeField] private TextMeshProUGUI _trinketDescription;

	private EventSystem _eventSystem;

	private Button _button;
	private bool _isActive;
	private bool _isEequiped;

	public bool IsEquiped { get { return _isEequiped; } }

	public bool ActiveSlot(Trinket trinket)
	{
		if(_trinket == trinket)
		{
			_isActive = true;
			_trinket.TackeTrinket();
			return true;
		}

		return false;
	}

	public void EquipTrinket(EquipedSlot slot)
	{
		_image.color = new Color(0.5f, 0.5f, 0.5f);
		_isEequiped = true;
		_trinket.Equip(slot);
	}

	public void UnequipTrinket()
	{
		_image.color = new Color(1f, 1f, 1f);
		_isEequiped = false;
		_trinket.Unequip();
	}

	public Trinket GetTrinket() { return _trinket; }

	public void SetText()
	{
		_trinketName.text = _trinket.Name;
		_trinketDescription.text = _trinket.Description;
	}

	private void OnEnable()
	{
		_button = GetComponent<Button>();
		_eventSystem = EventSystem.current;

		
	}

	// Start is called before the first frame update
	void Start()
    {
		_image.enabled = false;
		//_button.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
		if(_trinket == null)
		{
			return;
		}

		if (_trinket.IsTacken)
		{
			_isActive = true;
		}

		if (_isActive && !_image.enabled)
		{
			_image.sprite = _trinket.GetSprite();
			_image.enabled = true;
			_button.interactable = true;
		}

		if(_eventSystem.currentSelectedGameObject == _button.gameObject && _isActive)
		{
			SetText();
		}
		else
		{
			ClearText();
		}

    }

	private void ClearText()
	{
		_trinketName.text = string.Empty;
		_trinketDescription.text = string.Empty;
	}
}
