using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doors : MonoBehaviour
{
	[SerializeField] private BoxCollider2D _groundChecker;
	[SerializeField] private LayerMask _groundLayer;

	private Rigidbody2D _rb;
	private Vector2 _velocity;
	private bool _stop;

	public void StartBoosFight()
	{
		CloseDoors();
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
		if (Physics2D.OverlapBox(_groundChecker.transform.position, _groundChecker.size, 0, _groundLayer))
		{
			_velocity = Vector2.zero;
			_stop = true;
		}

		if (_stop)
		{
			_rb.velocity = Vector2.zero;
			return;
		}
		else
		{
			float speedDif = 1f;

			_velocity = Vector2.down * speedDif;

			_rb.AddForce(_velocity);
		}
    }

	private void FixedUpdate()
	{
		
	}

	private void CloseDoors()
	{
		_stop = false;
	}
}
