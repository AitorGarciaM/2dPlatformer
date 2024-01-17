using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : TransitionDestination
{
	public enum CheckPointWhen
	{
		ExternalCall,
		OnTriggerEnter
	}

	[SerializeField] private CheckPointWhen _checkPointWhen;

	private bool _chekPointSet = false;

	public void SetCheckPoint()
	{
		SceneController.Instance.SetCheckPoint(this);
		_chekPointSet = true;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if(_checkPointWhen == CheckPointWhen.OnTriggerEnter && !_chekPointSet)
		{
			PlayerController player = collision.GetComponent<PlayerController>();
			player.SetLastTransitionDestination(this);
			_chekPointSet = true;
		}
	}
}
