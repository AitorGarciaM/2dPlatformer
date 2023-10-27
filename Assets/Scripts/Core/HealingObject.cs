using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "HealingObject", menuName = "Items/Healing")]
public class HealingObject : ScriptableObject
{
	[SerializeField] private float _duration;
	[SerializeField] private float _helthPerSecond;

	public float Duration { get { return _duration; } }
	public float HealthPerSecond { get { return _helthPerSecond; } }
}
