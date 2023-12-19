using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class MenuNavigationManager : MonoBehaviour
{
	private static MenuNavigationManager s_isntance;

	private EventSystem _eventSystem;
	
	public static MenuNavigationManager Instance
	{
		get
		{
			if(s_isntance != null)
			{
				return s_isntance;
			}

			s_isntance = FindObjectOfType<MenuNavigationManager>();

			if(s_isntance != null)
			{
				return s_isntance;
			}

			Create();
			return s_isntance;
		}
	}

	private static void Create()
	{
		MenuNavigationManager prefabController = Resources.Load<MenuNavigationManager>("Prefabs/MenuNavigationManager");
		s_isntance = Instantiate(prefabController);
	}

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		_eventSystem = EventSystem.current;
	}

	private void Update()
	{
		
	}

	private IEnumerator ChangeMenuInternal(GameObject selectable)
	{
		EventSystem.current.SetSelectedGameObject(null);
		yield return null;
		EventSystem.current.SetSelectedGameObject(selectable);
	}

	public static void ChengeMenu(GameObject selectable)
	{
		Instance.StartCoroutine(Instance.ChangeMenuInternal(selectable));
	}
}
