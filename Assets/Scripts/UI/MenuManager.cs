using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
	private EventSystem _eventSystem;

	private void Awake()
	{
		_eventSystem = FindObjectOfType<EventSystem>();
	}

	public void PauseGame()
	{
		Time.timeScale = 0;
	}

	public void ContinueGame()
	{
		Time.timeScale = 1;
	}

	public void Quit()
	{
#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	public void ChangeMenu(GameObject selected)
	{
		Debug.Log("Changing Menu");

		_eventSystem.firstSelectedGameObject = null;
		_eventSystem.firstSelectedGameObject = selected;
	}
}
