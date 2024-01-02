using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lever : MonoBehaviour, Iinteractable
{
	[SerializeField] private CanvasGroup _interactGroup;
	[SerializeField] private Door _door;

	private LeverAnimationHandler _animHandler;
	private bool _interactable = true;

	public void Interact()
	{
		_animHandler.SetTrigger("Activate");
		_door.Open();
		_interactable = false;
	}

	private void Awake()
	{
		_animHandler = GetComponent<LeverAnimationHandler>();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "Player" && _interactable)
		{
			collision.GetComponent<PlayerController>().SetInteractable(this);
			_interactGroup.alpha = 1;
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.tag == "Player")
		{
			collision.GetComponent<PlayerController>().SetInteractable(null);
			_interactGroup.alpha = 0;
		}
	}
}
