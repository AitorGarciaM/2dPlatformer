using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSystem : MonoBehaviour
{
	[Header("Run")]
	[SerializeField] private float _maxSpeed;
	[SerializeField] private float _runAccelerationAmout;
	[SerializeField] private float _runDecelerationAmount;
	[SerializeField] private float _accelInAir;
	[SerializeField] private float _deccelInAir;

	[Space(5)]

	[SerializeField] private bool _doConserveMomontum;

	[Header("Jump")]
	[SerializeField] private float _jumpHeight;
	[SerializeField] private float _jumpTimeToApex;
	[SerializeField] private Transform _groundCheckPoint;
	[SerializeField] private Vector2 _groundCheckSize;

	[Header("Assist")]
	[SerializeField] private float _coyoteTime;	

	[Header("Layers & Tags")]
	[SerializeField] private LayerMask _groundLayer;

	private Rigidbody2D _rb;

	private Vector2 _movementVector;

	private float _gravityStrength;
	private float _gravityScale;
	private float _jumpForce;

	public float LastOnGroundTime { get; private set; }
	public float GravityScale { get { return _gravityScale; } }
	public float JumpHeight { get { return _jumpHeight; } }
	public float JumpTimeToApex { get { return _jumpTimeToApex; } }
	
	public bool IsGrounded { get; private set; }
	public bool IsJumping { get; private set; }

	public void SetDesiredDirection(Vector2 desiredDirection)
	{
		_movementVector = desiredDirection;
	}

	public void StartJump()
	{
		
	}

	private void Awake()
	{
		_rb = GetComponent<Rigidbody2D>();
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

	private void FixedUpdate()
	{
		Run(1);
		_movementVector.x = 0;
	}

	// Update is called once per frame
	void Update()
    {
		LastOnGroundTime -= Time.deltaTime;
		

		if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer))
		{
			IsGrounded = true;
			LastOnGroundTime = _coyoteTime;
		}
		else
		{
			IsGrounded = false;
		}

		if (IsJumping && _rb.velocity.y < 0)
		{
			IsJumping = false;
		}
	}
	
	private void Run(float lerpAmount)
	{
		float targetSpeed = _movementVector.x * _maxSpeed;

		targetSpeed = Mathf.Lerp(_rb.velocity.x, targetSpeed, lerpAmount);

		float accelRate = 0;

		if (IsGrounded)
		{
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? _runAccelerationAmout : _runDecelerationAmount;
		}
		else
		{
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? _runAccelerationAmout * _accelInAir : _runDecelerationAmount * _deccelInAir;
		}

		if (_doConserveMomontum && Mathf.Abs(_rb.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(_rb.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
		{
			accelRate = 0;
		}

		float speedDif = targetSpeed - _rb.velocity.x;

		float movement = speedDif * accelRate;

		_rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
		_movementVector = Vector2.zero;
	}

	public void Jump()
	{
		IsJumping = true;
		LastOnGroundTime = 0;
		float force = _jumpForce;
		if (_rb.velocity.y < 0)
		{
			force -= _rb.velocity.y;
		}

		_rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
	}

	public bool CanJump()
	{
		return LastOnGroundTime > 0 && !IsJumping;
	}

	private void OnValidate()
	{
		_gravityStrength = -(2 * _jumpHeight) / (_jumpTimeToApex * _jumpTimeToApex);

		_gravityScale = _gravityStrength / Physics2D.gravity.y;
		_jumpForce = Mathf.Abs(_gravityStrength) * _jumpTimeToApex;
	}

	#region Gizmos
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
	}
	#endregion
}
