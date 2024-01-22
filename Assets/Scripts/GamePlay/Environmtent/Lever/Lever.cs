using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lever : MonoBehaviour, Iinteractable
{
	[SerializeField] private CanvasGroup _interactGroup;
	[SerializeField] private Door _door;
	[SerializeField] private RandomAudioPlayer _leverPlayer;
	private LeverAnimationHandler _animHandler;
	private bool _interactable = true;

	public void Interact()
	{
		_animHandler.SetTrigger("Activate");
		_door.Open();
		_interactable = false;
	}

	public void PlayLever()
	{
		_leverPlayer.PlayRandomSound();
	}

	private void Awake()
	{
		_animHandler = transform.GetChild(0).GetComponent<LeverAnimationHandler>();
		_animHandler.SetLever(this);
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
