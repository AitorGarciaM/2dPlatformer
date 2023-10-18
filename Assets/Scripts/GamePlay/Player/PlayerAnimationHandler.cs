using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationHandler : AnimationHnadler
{
	private bool _attackActive = false;
	private bool _stopMovement = false;
	private bool _blockAttack = false;

	public bool IsAttackActive { get { return _attackActive; } }
	public bool IsMovementEnable { get { return _stopMovement; } }
	public bool IsAttackEnable { get { return _blockAttack; } }

    public void StartMoving()
	{
		_stopMovement = false;
	}

	public void EnableAttack()
	{
		_blockAttack = true;
	}

	public void DisableAttack()
	{
		_blockAttack = false;
	}

	public void StopMoving()
	{
		_stopMovement = true;
	}

	public void ActiveAttack()
	{
		_attackActive = true;
	}

	public void DeactiveAttack()
	{
		_attackActive = false;
	}
}
