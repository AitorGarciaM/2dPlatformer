using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimationHnadler : MonoBehaviour
{
	[SerializeField] protected Animator _animator;

	public bool GetBool(string name)
	{
		return _animator.GetBool(name);
	}

	public bool GetBool(int id)
	{
		return _animator.GetBool(id);
	}

	public int GetInteger(string name)
	{
		return _animator.GetInteger(name);
	}

	public int GetInteger(int id)
	{
		return _animator.GetInteger(id);
	}

	public float GetFloat(string name)
	{
		return _animator.GetFloat(name);
	}

	public float GetFloat(int id)
	{
		return _animator.GetFloat(id);
	}

	public void SetBool(string name, bool value)
	{
		_animator.SetBool(name, value);
	}

	public void SetBool(int id, bool value)
	{
		_animator.SetBool(id, value);
	}

	public void SetInteger(string name, int value)
	{
		_animator.SetInteger(name, value);
	}

	public void SetInteger(int id, int value)
	{
		_animator.SetInteger(id, value);
	}

	public void SetFloat(string name, float value)
	{
		_animator.SetFloat(name, value);
	}

	public void SetFloat(int id, float value)
	{
		_animator.SetFloat(id, value);
	}

	public void SetTrigger(string name)
	{
		_animator.SetTrigger(name);
	}

	public void SetTrigger(int id)
	{
		_animator.SetTrigger(id);
	}
}
