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
	[SerializeField] private float _jumpHungTimeThreshold;
	[SerializeField, Range(1, 10)] private float jumpCutGravityMultiply;
	[SerializeField] private float _maxFallSpeed;
	[SerializeField] private float _fallGravityMulty;
	[SerializeField] private float _jumpHangGravityMulty;
	[SerializeField] private Transform _groundCheckPoint;
	[SerializeField] private Vector2 _groundCheckSize;

	[Header("Assist")]
	[SerializeField] private float _coyoteTime;
	[SerializeField] private float _slopeCheckDistance;

	[Header("Physics")]
	[SerializeField] CircleCollider2D _circleCollider;
	[SerializeField] PhysicsMaterial2D _noFriction;
	[SerializeField] PhysicsMaterial2D _fullFriction;

	[Header("Layers & Tags")]
	[SerializeField] private LayerMask _groundLayer;

	private Rigidbody2D _rb;

	private Vector2 _movementVector;

	private float _gravityStrength;
	private float _gravityScale;
	private float _jumpForce;
	private float _slopeDownAngle;
	private float _slopeDownAnlgeOld;

	private bool _isOnSlope;
	private bool _isJumpCut;

	public float LastOnGroundTime { get; private set; }
	public float GravityScale { get { return _gravityScale; } }
	public float JumpHeight { get { return _jumpHeight; } }
	public float JumpTimeToApex { get { return _jumpTimeToApex; } }
	
	public bool IsGrounded { get; private set; }
	public bool IsLanding { get; private set; }
	public bool IsOnSlope { get { return _isOnSlope; } }
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
		SlopeCheck();
		Run(1);
		_movementVector.x = 0;
	}

	// Update is called once per frame
	void Update()
    {
		LastOnGroundTime -= Time.deltaTime;

		#region Collision Check
		if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer))
		{
			if(!IsGrounded)
			{
				IsLanding = true;
			}
			else
			{
				IsLanding = false;
			}

			IsGrounded = true;
			LastOnGroundTime = _coyoteTime;
		}
		else
		{
			IsGrounded = false;
		}
		#endregion

		#region Jump Check
		if (IsJumping && _rb.velocity.y < 0)
		{
			IsJumping = false;
		}

		if(LastOnGroundTime > 0 && !IsJumping)
		{
			_isJumpCut = false;
		}
		#endregion

		#region Gravity
		if(_isJumpCut)
		{
			Debug.Log("Jump is Cut");
			// Higer gravity if jump button is released.
			SetGravityScale(_gravityScale * jumpCutGravityMultiply);
			_rb.velocity = new Vector2(_rb.velocity.x, Mathf.Max(_rb.velocity.y, -_maxFallSpeed));
		}
		else if(IsJumping && Mathf.Abs(_rb.velocity.y) < _jumpHungTimeThreshold)
		{
			SetGravityScale(_gravityScale * _jumpHangGravityMulty);
		}
		else if(_rb.velocity.y < 0)
		{
			Debug.Log("falling");
			// Higer gravity if it's falling.
			SetGravityScale(_gravityScale * _fallGravityMulty);
			_rb.velocity = new Vector2(_rb.velocity.x, Mathf.Max(_rb.velocity.y, -_maxFallSpeed));
		}
		else
		{
			Debug.Log("Default gravity");
			// Defalut gravity if it's grounded.
			SetGravityScale(_gravityScale);
		}
		#endregion
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

	private void SlopeCheck()
	{
		Vector2 checkPos = transform.position - new Vector3(0.0f, _circleCollider.radius);

		SlopeCheckHorizontal(checkPos);
		SlopeCheckVertical(checkPos);
	}

	private void SlopeCheckHorizontal(Vector2 checkPos)
	{
		RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right, _slopeCheckDistance, _groundLayer);
		RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right, _slopeCheckDistance, _groundLayer);

		if(slopeHitFront)
		{
			_isOnSlope = true;
		}
		else if(slopeHitBack)
		{
			_isOnSlope = true;
		}
		else
		{
			_isOnSlope = false;
		}
	}

	private void SlopeCheckVertical(Vector2 checkPos)
	{
		RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, _slopeCheckDistance, _groundLayer);

		if(hit)
		{
			_slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

			if(_slopeDownAngle != _slopeDownAnlgeOld)
			{
				_isOnSlope = true;
			}
		}

		if(_isOnSlope && _movementVector.x == 0.0f)
		{
			_rb.sharedMaterial = _fullFriction;
		}
		else
		{
			_rb.sharedMaterial = _noFriction;
		}
	}

	private void SetGravityScale(float scale)
	{
		_rb.gravityScale = scale;
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

	public bool CanJumpCut()
	{
		return IsJumping && _rb.velocity.y > 0;
	}

	public void OnStopJump()
	{
		if(CanJumpCut())
		{
			_isJumpCut = true;
		}
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
