using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionPoint : MonoBehaviour
{
	public enum Transition_Type
	{
		DiferentZone, DifferentNonGameplayScene, SameScene
	}
	public enum Transition_When
	{
		ExternalCall, InteractPressed, OnTriggerEnter
	}

	[SerializeField] private GameObject _transitioningGameObject;
	[SerializeField] private Transition_Type _transitionType;
	[SerializeField] private Transition_When _transitionWhen;
	[SerializeField] private string _newSceneName;
	[SerializeField] private TransitionDestination.Destination_Tag _transitionDestinationTag;
	[SerializeField] private bool _resetInputValuesOnTransition = true;

	public string NewSceneName{ get{ return _newSceneName; } }
	public Transition_Type TransitionType { get { return _transitionType; } }
	public TransitionDestination.Destination_Tag TransitionDestinationTag { get { return _transitionDestinationTag; } }
	public bool ResetInputValuesOnTransition { get { return _resetInputValuesOnTransition; } }

	private bool _transitioningGameObjectPresent;

    // Start is called before the first frame update
    void Start()
    {
        if(_transitionWhen == Transition_When.ExternalCall)
		{
			_transitioningGameObjectPresent = true;
		}
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if(collision.gameObject == _transitioningGameObject)
		{
			_transitioningGameObjectPresent = true;

			if(ScreenFader.Instance.IsFading || SceneController.Instance.Transitioning)
			{
				return;
			}

			MovementSystem movementSystem = collision.GetComponent<MovementSystem>();

			if(movementSystem != null)
			{
				movementSystem.enabled = false;
			}
					   
			TransitionInternal();
		}
	}

	private void TransitionInternal()
	{
		switch (_transitionType)
		{
			case Transition_Type.SameScene:
				TransitionDestination destination = GetDestination(_transitionDestinationTag);
				_transitioningGameObject.transform.position = destination.transform.position;
				break;
			case Transition_Type.DiferentZone:
			case Transition_Type.DifferentNonGameplayScene:
				SceneController.Instance.TransitionToScene(this);
				break;
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if(collision.gameObject == _transitioningGameObject)
		{
			if(ScreenFader.Instance.IsFading || SceneController.Instance.Transitioning)
			{
				return;
			}

			if (_transitionWhen == Transition_When.OnTriggerEnter)
			{
				_transitioningGameObjectPresent = false;
			}
		}
	}

	public void Transition()
	{
		if(!_transitioningGameObjectPresent)
		{
			return;
		}

		if(_transitionWhen == Transition_When.ExternalCall)
		{
			TransitionInternal();
		}
	}

	private TransitionDestination GetDestination(TransitionDestination.Destination_Tag destinationTag)
	{
		TransitionDestination[] entrances = FindObjectsOfType<TransitionDestination>();

		for (int i = 0; i < entrances.Length; i++)
		{
			if (entrances[i].DestinationTag == destinationTag)
			{
				return entrances[i];
			}
		}

		Debug.LogWarning("No entrances was found with the tag " + destinationTag + ".");
		return null;
	}
}
