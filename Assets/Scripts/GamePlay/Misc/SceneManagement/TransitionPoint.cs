using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionPoint : MonoBehaviour
{
	public enum Transition_Type
	{
		DiferentZone, DifferentNonGameplayScene, SameScene
	}

	[SerializeField] private GameObject _transitioningGameObject;
	[SerializeField] private Transition_Type _transitionType;
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

			TransitionInternal();
		}
	}

	private void TransitionInternal()
	{
		switch (_transitionType)
		{
			case Transition_Type.SameScene:
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
			_transitioningGameObjectPresent = false;
		}
	}
}
