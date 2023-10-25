using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doors : MonoBehaviour
{
	[SerializeField] private BoxCollider2D _groundChecker;
	[SerializeField] private BoxCollider2D _wallChecker;
	[SerializeField] private LayerMask _groundLayer;
	[SerializeField] private LayerMask _wallLayer;

	private Rigidbody2D _rb;
	private Vector2 _velocity;
	private bool _stop;
	private bool _openDoors;

	public void StartBoosFight()
	{
		CloseDoors();
	}

	public void EndBoosFight()
	{
		OpenDoors();
	}

    // Start is called before the first frame update
    void Start()
    {
		_rb = GetComponent<Rigidbody2D>();
		_stop = true;
    }

    // Update is called once per frame
    void Update()
    {
		if (Physics2D.OverlapBox(_groundChecker.transform.position, _groundChecker.size, 0, _groundLayer) && _velocity.y < 0)
		{
			_velocity = Vector2.zero;
			_stop = true;
		}
		else if (Physics2D.OverlapBox(_wallChecker.transform.position, _wallChecker.size, 0, _wallLayer) && _velocity.y > 0)
		{
			_velocity = Vector2.zero;
			_stop = true;
		}

		if (_stop)
		{
			_rb.velocity = Vector2.zero;
			return;
		}
		else if(!_openDoors)
		{
			float speedDif = 1f;

			_velocity = Vector2.down * speedDif;

			_rb.AddForce(_velocity);
		}
		else if(_openDoors)
		{
			float speedDif = 1f;

			_velocity = Vector2.up * speedDif;

			_rb.AddForce(_velocity);
		}
    }

	private void FixedUpdate()
	{
		
	}

	private void CloseDoors()
	{
		_stop = false;
		_openDoors = false;
	}

	private void OpenDoors()
	{
		_stop = false;
		_openDoors = true;
	}
}
