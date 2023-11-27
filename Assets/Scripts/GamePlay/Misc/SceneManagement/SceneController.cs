using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
	private static SceneController s_instance;
	private bool _transitioning;
	private Scene _currentZoneScene;
	private TransitionDestination.Destination_Tag _zoneRestartDestinationTag;
	private CheckPoint _checkPoint;

	private string _checkPointSceneName;
	private TransitionDestination.Destination_Tag _checkPointDestinationTag;

	public static SceneController Instance
	{
		get
		{
			if (s_instance != null)
			{
				return s_instance;
			}

			s_instance = FindObjectOfType<SceneController>();

			if(s_instance != null)
			{
				return s_instance;
			}

			Create();

			return s_instance;
		}
	}
	   
	public bool Transitioning { get { return _transitioning; } }

	private static void Create()
	{
		SceneController controllerPrefab = Resources.Load<SceneController>("Prefabs/SceneController");
		s_instance = Instantiate(controllerPrefab);
	}

	private void Awake()
	{
		if(Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		DontDestroyOnLoad(gameObject);
	}

	private IEnumerator Transition(string newSceneName, bool resetInputValues, TransitionDestination.Destination_Tag destinationTag, TransitionPoint.Transition_Type transitionType = TransitionPoint.Transition_Type.DiferentZone)
	{
		_transitioning = true;
		DataPersistenersManager.SaveAllData();

		PlayerController playerController = FindObjectOfType<PlayerController>();
		//playerController.PauseControl();
		playerController.TrasitionToNewScene();
		yield return StartCoroutine(ScreenFader.Instance.FadeSceneOut());
		Debug.Log("End fade out.");
		yield return SceneManager.LoadSceneAsync(newSceneName);
		Debug.Log("End new scene loading");
		DataPersistenersManager.LoadAllData();
		TransitionDestination entrance = GetDestination(destinationTag);
		SetEntringGameObjectLocation(entrance);
		SetUpNewScene(transitionType, entrance);
		if (entrance != null)
		{
			entrance.OnReachDestination.Invoke();
		}
		yield return StartCoroutine(ScreenFader.Instance.FadeSceneIn());

		//playerController.RestartControl();
		_transitioning = false;
	}

	private IEnumerator MenuTransition(string newSceneName, bool resetInputValues, TransitionDestination.Destination_Tag destinationTag, TransitionPoint.Transition_Type transitionType = TransitionPoint.Transition_Type.DiferentZone)
	{
		_transitioning = true;
		yield return StartCoroutine(ScreenFader.Instance.FadeSceneOut());
		yield return SceneManager.LoadSceneAsync(newSceneName);
		Debug.Log("End new scene loading");
		DataPersistenersManager.LoadAllData();
		TransitionDestination entrance = GetDestination(destinationTag);
		SetEntringGameObjectLocation(entrance);
		SetUpNewScene(transitionType, entrance);
		if (entrance != null)
		{
			entrance.OnReachDestination.Invoke();
		}
		yield return StartCoroutine(ScreenFader.Instance.FadeSceneIn());

		//playerController.RestartControl();
		_transitioning = false;
	}

	private TransitionDestination GetDestination(TransitionDestination.Destination_Tag destinationTag)
	{
		TransitionDestination[] entrances = FindObjectsOfType<TransitionDestination>();

		for (int i = 0; i < entrances.Length; i++)
		{
			if(entrances[i].DestinationTag == destinationTag)
			{
				return entrances[i];
			}
		}

		Debug.LogWarning("No entrances was found with the tag " + destinationTag + " tag.");
		return null;
	}

	private void SetEntringGameObjectLocation(TransitionDestination entrance)
	{
		if(entrance == null)
		{
			Debug.LogWarning("Entring transform's location has not been set.");
			return;
		}

		Transform entranceLocation = entrance.transform;
		Transform enteringTransform = entrance.TransitioningGameObject.transform;
		enteringTransform.position = entranceLocation.position;
		enteringTransform.rotation = entranceLocation.rotation;
	}

	private void SetUpNewScene(TransitionPoint.Transition_Type transitionType, TransitionDestination entrance)
	{
		if(entrance == null)
		{
			Debug.LogWarning("Restart information has not been set.");
			return;
		}

		if(transitionType == TransitionPoint.Transition_Type.DiferentZone)
		{
			_currentZoneScene = entrance.gameObject.scene;
			_zoneRestartDestinationTag = entrance.DestinationTag;
		}
	}

	public void TransitionToScene(TransitionPoint transitionPoint)
	{
		Instance.StartCoroutine(Transition(transitionPoint.NewSceneName, transitionPoint.ResetInputValuesOnTransition, transitionPoint.TransitionDestinationTag, transitionPoint.TransitionType));
	}

	public void TransitionToStart()
	{
		Instance.StartCoroutine(MenuTransition("Tutorial", true, TransitionDestination.Destination_Tag.CheckPoint, TransitionPoint.Transition_Type.DiferentZone));
	}

	public void SetCheckPoint(CheckPoint checkPoint)
	{
		_checkPoint = checkPoint;
		_checkPointSceneName = checkPoint.gameObject.scene.name;
		_checkPointDestinationTag = checkPoint.DestinationTag;
	}

	public void TransitionToCheckPoint()
	{
		Instance.StartCoroutine(Transition(_checkPointSceneName, true, _checkPointDestinationTag, TransitionPoint.Transition_Type.DiferentZone));
	}
}
