using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TransitionDestination : MonoBehaviour
{
	public enum Destination_Tag
	{
		A, B, C, D, CheckPoint
	}

	[SerializeField] protected Destination_Tag _destinationTag;
	[SerializeField] protected GameObject _transitioningGameObject;

	public UnityEvent OnReachDestination;

	public Destination_Tag DestinationTag { get { return _destinationTag; } }
	public GameObject TransitioningGameObject { get { return _transitioningGameObject; } }
}
