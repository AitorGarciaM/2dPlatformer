using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlickeringLight : MonoBehaviour
{
	[SerializeField] private float _minIntensity;
	[SerializeField] private float _maxIntensity;

	private Light2D _light2D;
	private float _currentTime;

    // Start is called before the first frame update
    void Start()
    {
		_light2D = GetComponent<Light2D>();
    }

    // Update is called once per frame
    void Update()
    {
		float waitTime = Random.Range(0, 0.8f);

		_currentTime += Time.deltaTime;

		if(_currentTime >= waitTime)
		{
			_light2D.intensity = Random.Range(_minIntensity, _maxIntensity);
			_currentTime = 0;
		}
    }
}
