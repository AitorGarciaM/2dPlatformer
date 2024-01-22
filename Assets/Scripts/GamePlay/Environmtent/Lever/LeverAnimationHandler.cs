using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverAnimationHandler : AnimationHnadler
{
	private Lever _lever;

	public void SetLever(Lever lever)
	{
		_lever = lever;
	}

    public void PlayLever()
	{
		_lever.PlayLever();
	}
}
