using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class NavigationPanel : MonoBehaviour
{
	[SerializeField] UnityEvent _onBack;

	private EventSystem _eventSystem;
	private InputAction _cancel;

    // Start is called before the first frame update
    void Start()
    {
		_eventSystem = EventSystem.current;
		InputSystemUIInputModule uiInput = (InputSystemUIInputModule)_eventSystem.currentInputModule;

		_cancel = uiInput.cancel.action;
	}

    // Update is called once per frame
    void Update()
    {
        if(_cancel.WasPerformedThisFrame())
		{
			_onBack.Invoke();
		}
    }
}
