using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatAnimator : AnimationHnadler
{
	private BatController _batController;

	public void Fly()
	{
		_batController.PlayFly();
	}

	private void Start()
	{
		_batController = transform.parent.GetComponent<BatController>();
	}
}
