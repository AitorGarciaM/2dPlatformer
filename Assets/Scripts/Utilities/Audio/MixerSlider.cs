using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MixerSlider : MonoBehaviour
{
	[SerializeField] private AudioMixer _audioMixer;
	[SerializeField] private string _audioMixerParameter;
	[SerializeField] private float _maxAtenuationValue = 0.0f;
	[SerializeField] private float _minAtenuationValue = -80.0f;

	private Slider _slider;

	private void Awake()
	{
		_slider = GetComponent<Slider>();

		float value;

		_audioMixer.GetFloat(_audioMixerParameter, out value);

		_slider.value = (value - _minAtenuationValue) / (_maxAtenuationValue - _minAtenuationValue);

		_slider.onValueChanged.AddListener(SetVolume);
	}

	public void SetVolume(float value)
	{
		_audioMixer.SetFloat(_audioMixerParameter, _minAtenuationValue + value * (_maxAtenuationValue - _minAtenuationValue));
	}
}
