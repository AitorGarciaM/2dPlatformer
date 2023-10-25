using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAnimationHandler : AnimationHnadler
{
	private int _rampageCount;
	private bool _attackEnds = true;
	private bool _flame = false;
	private bool _smashing = false;
	private bool _landing = false;
	private bool _invincible = false;
	private bool _flipping;

	public void Start()
	{
		_flipping = true;
	}

	public bool IsAttackEnding { get { return _attackEnds; } }
	public bool IsThrowingFlame { get { return _flame; } }
	public bool IsLanding { get { return _landing; } }
	public bool IsSmashing { get { return _smashing; } }
	public bool IsInvincible { get { return _invincible; } }
	public bool IsFlipActive { get { return _flipping; } }

	public bool Flip { get; private set; }

	public void StartAttack()
	{
		_attackEnds = false;
	}

	public void AttackEnds()
	{
		_attackEnds = true;
	}

	public void FlameStart()
	{
		_flame = true;
	}

	public void FlameEnds()
	{
		_flame = false;
	}

	public void SmashStart()
	{
		_smashing = true;
	}

	public void SmashEnds()
	{
		_smashing = false;
	}

	public void LandStart()
	{
		_landing = true;
	}

	public void LandEnds()
	{
		_landing = false;
	}

	public void StartInvincibility()
	{
		_invincible = true;
	}

	public void EndInvincibility()
	{
		_invincible = false;
	}

	public void StopFlipping()
	{
		_flipping = false;
	}

	public void PlayFlipping()
	{
		_flipping = true;
	}
}
