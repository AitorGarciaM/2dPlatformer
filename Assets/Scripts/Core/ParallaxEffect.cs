using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
	[SerializeField] private float _parallaxMultiplier;
	[SerializeField] private bool _followCamera = true;

	private Transform _cameraTransform;
	private Vector3 _previousCameraPosition;
	private float _spriteWidth, _startPosition;

	// Start is called before the first frame update
	void Start()
    {
		_cameraTransform = Camera.main.transform;
		_previousCameraPosition = _cameraTransform.position;
		_spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x;
		_startPosition = transform.position.x;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		if(_followCamera)
		{
			FollowCamera();
		}
		else
		{
			StaticParallax();
		}
    }

	private void FollowCamera()
	{
		float deltaX = (_cameraTransform.position.x - _previousCameraPosition.x) * _parallaxMultiplier;
		float moveAmout = _cameraTransform.position.x * (1 - _parallaxMultiplier);
		transform.Translate(deltaX, 0, 0);
		_previousCameraPosition = _cameraTransform.position;

		if (moveAmout > (_startPosition + _spriteWidth))
		{
			transform.Translate(_spriteWidth, 0, 0);

			_startPosition += _spriteWidth;
		}
		else if (moveAmout < (_startPosition - _spriteWidth))
		{
			transform.Translate(-_spriteWidth, 0, 0);

			_startPosition -= _spriteWidth;
		}
	}

	private void StaticParallax()
	{
		float speed = -0.5f * _parallaxMultiplier * Time.fixedDeltaTime;

		transform.position = new Vector3(transform.position.x + speed, transform.position.y, transform.position.z);

		if(transform.position.x <= -(_spriteWidth))
		{
			transform.position = new Vector3(_spriteWidth, transform.position.y, transform.position.z);
		}
	}

}
