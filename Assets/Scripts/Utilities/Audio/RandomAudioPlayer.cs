using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(AudioSource))]
public class RandomAudioPlayer : MonoBehaviour
{
	[SerializeField] private AudioClip _clip;
	[SerializeField] private bool _randomizePitch = false;
	[SerializeField] private float _randomPitch = 0.2f;

	private AudioSource _audioSource;

	private void Awake()
	{
		_audioSource = GetComponent<AudioSource>();
	}

	public void PlayRandomSound()
	{
		if(_randomizePitch)
		{
			_audioSource.pitch = Random.Range(1.0f - _randomPitch, 1.0f + _randomPitch);
		}

		_audioSource.PlayOneShot(_clip);
	}

	public void Stop()
	{
		_audioSource.Stop();
	}
}
