using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TrinketMenuManager : MonoBehaviour
{
	[SerializeField]
	private GameObject _firstSelected;
	[SerializeField] TextMeshProUGUI _firstButtonIndicator;
	[SerializeField] TextMeshProUGUI _secondButtonIndicator;
	[SerializeField] TextMeshProUGUI _thirdButtonIndicator;

	private GameObject _lastSelected;

	private EventSystem _eventSystem;
	private InputSystemUIInputModule _input;
	
	public void SetLastSelected(GameObject lastSelected)
	{
		_lastSelected = lastSelected;
		_firstButtonIndicator.text = "Equip";
		_thirdButtonIndicator.text = "Cancel";
	}
	
	public void ResetText()
	{
		_firstButtonIndicator.text = "Select";
		_thirdButtonIndicator.text = "Exit";
	}

	private void OnEnable()
	{
		_eventSystem = EventSystem.current;
		_input = FindObjectOfType<InputSystemUIInputModule>();


		_eventSystem.SetSelectedGameObject(_firstSelected);
	}

	private void OnDisable()
	{
	}

	private void OnCancelInput()
	{
		if (_lastSelected != null)
		{
			_eventSystem.SetSelectedGameObject(_lastSelected);
			_firstButtonIndicator.text = "Select";
			_thirdButtonIndicator.text = "Exit";
			_lastSelected = null;
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	// Start is called before the first frame update
	void Start()
    {
		_firstButtonIndicator.text = "Select";
		_thirdButtonIndicator.text = "Exit";
	}

    // Update is called once per frame
    void Update()
    {
		if(_input.cancel.action.WasPerformedThisFrame())
		{
			OnCancelInput();
		}
    }
}
