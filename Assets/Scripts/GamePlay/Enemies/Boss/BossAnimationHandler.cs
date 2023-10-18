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

	public bool IsAttackEnding { get { return _attackEnds; } }
	public bool IsThtowingFlame { get { return _flame; } }
	public bool IsLanding { get { return _landing; } }
	public bool IsSmashing { get { return _smashing; } }

	public bool Flip { get; private set; }

	public void StartAttack()
	{
		_attackEnds = false;
	}

	public void AttackEnds()
	{
		_attackEnds = true;
	}

	public void Rampage()
	{
		_rampageCount++;

		if(_rampageCount % 2 == 0)
		{
			Flip = true;
		}
		else
		{
			Flip = false;
		}

		if(_rampageCount >= 8)
		{
			_rampageCount = 0;
			_attackEnds = true;
			Flip = false;
		}
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
}
