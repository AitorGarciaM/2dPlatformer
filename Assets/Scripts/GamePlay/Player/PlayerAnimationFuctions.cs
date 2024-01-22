using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationFuctions : MonoBehaviour
{
	[SerializeField] private AnimationHnadler _animationHandler;

    public void FootSteps()
	{
		PlayerAnimationHandler playerAnimationHandler = (PlayerAnimationHandler)_animationHandler;

		playerAnimationHandler.FootStep();
	}
}
