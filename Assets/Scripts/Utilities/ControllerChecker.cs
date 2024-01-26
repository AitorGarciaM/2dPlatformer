using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerChecker : MonoBehaviour
{
	public enum ControllerType
	{
		Keyboard,
		Gamepad
	}

	private static ControllerChecker s_instance;

	private ControllerChecker _oldInstanceToDestroy;
	private PlayerInput _input;
	private ControllerType _controllerType;

	public static ControllerChecker Instance
	{
		get
		{

			if(s_instance != null)
			{
				return s_instance;
			}

			s_instance = FindObjectOfType<ControllerChecker>();

			if(s_instance != null)
			{
				return s_instance;
			}

			Create();

			return s_instance;
		}
	}

	public ControllerType CurrentControllerType { get { return _controllerType; } }

	public void OnControllerChanges()
	{
		if (_input == null)
		{
			return;
		}

		Instance.OnControllChangesInternal();
	}

	private static void Create()
	{
		ControllerChecker controllerPrefab = Resources.Load<ControllerChecker>("Prefabs/ControllerChecker");

		s_instance = Instantiate(controllerPrefab);
	}

	private void Awake()
	{
		if (Instance != null && Instance.GetInstanceID() != this.GetInstanceID())
		{
			if(_controllerType != Instance._controllerType)
			{
				_controllerType = Instance._controllerType;
			}

			_oldInstanceToDestroy = Instance;
		}

		_input = FindObjectOfType<PlayerInput>();
	}

	// Start is called before the first frame update
	void Start()
    {
		DontDestroyOnLoad(this.gameObject);

		if (_oldInstanceToDestroy != null)
		{
			Destroy(_oldInstanceToDestroy.gameObject);
		}
	}

	private void OnControllChangesInternal()
	{
		if (_input.currentControlScheme == "Keyboard")
		{
			_controllerType = ControllerType.Keyboard;
		}
		else if (_input.currentControlScheme == "Gamepad")
		{
			_controllerType = ControllerType.Gamepad;
		}
	}
}
