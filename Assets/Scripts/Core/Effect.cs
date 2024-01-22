using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect : ScriptableObject
{
	public abstract void Apply(GameObject gameObject);
	public abstract void Disapply(GameObject gameObject);
	public abstract string GetValue();
}
