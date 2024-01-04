using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladePendulum : MonoBehaviour
{
	[SerializeField] private float _speed;
	[SerializeField] private float _maxAngle;
	[SerializeField] private PolygonCollider2D _bladeCollider;
	[SerializeField] private LayerMask _playerMask;
	
	private float _angle;

    // Start is called before the first frame update
    void Start()
    {
		
    }

	private void FixedUpdate()
	{
		// Check if hitable is hitted.
		ContactFilter2D contactFilter = new ContactFilter2D();
		contactFilter.layerMask = _playerMask;
		contactFilter.useTriggers = true;
		List<Collider2D> colliders = new List<Collider2D>();

		if(Physics2D.OverlapCollider(_bladeCollider, contactFilter,colliders) != 0)
		{
			foreach(var collider in colliders)
			{
				IHitable hitable;
				if(collider.gameObject.TryGetComponent<IHitable>(out hitable))
				{
					hitable.Hit(40);
				}
			}
		}

		RotatePendulum();
	}

	// Update is called once per frame
	void Update()
    {
		
    }

	private void RotatePendulum()
	{
		float angle = _maxAngle * Mathf.Sin(Time.time * _speed);
		transform.localRotation = Quaternion.Euler(0, 0, angle);
	}
}
