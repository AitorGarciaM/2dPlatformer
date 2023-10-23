using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShaker : MonoBehaviour
{
	public static CameraShaker Instance { get; private set; }

	[SerializeField] CinemachineImpulseSource _impulse;

	public void ShakeCamera(float intensity)
	{
		_impulse.GenerateImpulseWithForce(intensity);
	}

	private void Awake()
	{
		Instance = this;

	}
}
