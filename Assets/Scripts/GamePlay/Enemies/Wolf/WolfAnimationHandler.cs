using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfAnimationHandler : AnimationHnadler
{
    public bool CanMove { get; private set; }

	public void StopMove()
	{
		CanMove = false;
	}

	public void StartMove()
	{
		CanMove = true;
	}
}
