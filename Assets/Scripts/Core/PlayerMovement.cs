using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
	[Header("Run")]
	[SerializeField] private float _maxSpeed;
	[SerializeField] private float _runAccelerationAmout;
	[SerializeField] private float _runDecelerationAmount;
	[Space(5)]
	[SerializeField] private bool _doConserveMomontum;

	[Header("Jump")]
	[SerializeField] private float _jumpHeight;
	[SerializeField] private float _jumpTimeToApex;
	[SerializeField] private Transform _groundCheckPoint;
	[SerializeField] private Vector2 _groundCheckSize;

	[Header("Assist")]
	[SerializeField] private float _coyoteTime;
	[SerializeField] private float _jumpInputBufferTime;

	private Rigidbody2D _rb;
	private PlayerAction _input = null;
	private Vector2 _moveVector = Vector2.zero;
	private float _gravityStrength;
	private float _gravityScale;
	private float _jumpForce;
	private float _timeToEndAttack;

	[Header("Visual Settings")]
	[SerializeField] private SpriteRenderer _spriteRenderer;
	[SerializeField] private Animator _animationHandler;

	[Header("Layers & Tags")]
	[SerializeField] private LayerMask _groundLayer;
	[Space(10)]
	[SerializeField] private Transform _attackArea;

	private bool _jump;
	private bool _isJumpCut;
	private bool _isFacingLeft;
	private bool _stopMovement;

	public float LastOnGroundTime { get; private set; }
	public float LastPressedJumpTime { get; private set; }

	public bool IsJumping { get; private set; }

	private void Awake()
	{
		_input = new PlayerAction();
		_rb = GetComponent<Rigidbody2D>();

		_isFacingLeft = false;
		_attackArea.gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		_input.Enable();
		_input.Player.Movement.performed += OnMovementPerformed;
		_input.Player.Movement.canceled += OnMovementCancelled;
		_input.Player.Jump.started += OnJumpStarted;
		_input.Player.Jump.canceled += OnJumpCanceled;
		_input.Player.Attack.started += OnAttackStarted;
	}

	private void OnDisable()
	{
		_input.Disable();
		_input.Player.Movement.performed -= OnMovementPerformed;
		_input.Player.Movement.canceled -= OnMovementCancelled;
		_input.Player.Jump.started -= OnJumpStarted;
		_input.Player.Jump.canceled -= OnJumpCanceled;
		_input.Player.Attack.started -= OnAttackStarted;
	}

	#region Input

	private void OnMovementPerformed(InputAction.CallbackContext value)
	{
		_moveVector = value.ReadValue<Vector2>();
	}

	private void OnMovementCancelled(InputAction.CallbackContext value)
	{
		_moveVector = Vector2.zero;
	}

	private void OnJumpStarted(InputAction.CallbackContext context)
	{
		OnJumpingInput();
	}

	private void OnJumpCanceled(InputAction.CallbackContext context)
	{
		OnJumpingUpInput();
	}

	public void OnAttackStarted(InputAction.CallbackContext context)
	{
		_stopMovement = true;
		_timeToEndAttack = 0;
		_animationHandler.SetTrigger("Attack");
		_attackArea.gameObject.SetActive(true);
	}
	
	#endregion

	// Jump performed;
	public void OnJumpingInput()
	{
		LastPressedJumpTime = _jumpInputBufferTime;
		_animationHandler.SetTrigger("Jump");
	}

	// Jump jumpCanceled.
	public void OnJumpingUpInput()
	{
		if(CanJumpCut())
		{
			_isJumpCut = true;
		}
	}

	private void Update()
	{
		LastOnGroundTime -= Time.deltaTime;
		LastPressedJumpTime -= Time.deltaTime;
		_timeToEndAttack += Time.deltaTime;

		if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer))
		{
			if(LastOnGroundTime < -0.1f)
			{
				// Land animation.
			}

			LastOnGroundTime = _coyoteTime;
		}

		if(IsJumping && _rb.velocity.y < 0)
		{
			IsJumping = false;
		}

		if(CanJump() && LastPressedJumpTime > 0)
		{
			IsJumping = true;
			_isJumpCut = false;
			Jump();
		}
	}

	private void LateUpdate()
	{
		_spriteRenderer.flipX = _isFacingLeft;
		int attackFrame = GetCurrentFrame("Attack");

		if(attackFrame == 4)
		{
			_attackArea.gameObject.SetActive(false);
		}

		StartMovement(new string[] {"Attack", "Combo", "TakeDamage"});

		if (_isFacingLeft)
			_attackArea.localPosition = new Vector2(-0.14f, _attackArea.localPosition.y);
		else
			_attackArea.localPosition = new Vector2(0.14f, _attackArea.localPosition.y);

		_animationHandler.SetFloat("Velocity_X", Mathf.Abs(_rb.velocity.x));
		_animationHandler.SetFloat("Velocity_Y", _rb.velocity.y);
		_animationHandler.SetFloat("Time_to_end_Attack", _timeToEndAttack);
	}

	private void FixedUpdate()
	{
		if (!_stopMovement)
			Run(1);
		else
			_rb.velocity = new Vector2(0, _rb.velocity.y);
	}

	private void OnValidate()
	{
		_gravityStrength = -(2 * _jumpHeight) / (_jumpTimeToApex * _jumpTimeToApex);

		_gravityScale = _gravityStrength / Physics2D.gravity.y;

		_jumpForce = Mathf.Abs(_gravityStrength) * _jumpTimeToApex;
	}

	private void Run(float lerpAmount)
	{
		float targetSpeed = _moveVector.x * _maxSpeed;

		if(targetSpeed > 0)
		{
			_isFacingLeft = false;
		}
		else if(targetSpeed < 0)
		{
			_isFacingLeft = true;
		}
		
		targetSpeed = Mathf.Lerp(_rb.velocity.x, targetSpeed, lerpAmount);
		
		float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? _runAccelerationAmout : _runDecelerationAmount;

		if(_doConserveMomontum && Mathf.Abs(_rb.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(_rb.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
		{
			accelRate = 0;
		}

		float speedDif = targetSpeed - _rb.velocity.x;

		float movement = speedDif * accelRate;

		_rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
	}

	private void Jump()
	{
		LastPressedJumpTime = 0;
		LastOnGroundTime = 0;
		float force = _jumpForce;
		if(_rb.velocity.y < 0)
		{
			force -= _rb.velocity.y;
		}

		_rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
	}

	private bool CanJump()
	{
		return LastOnGroundTime > 0 && !IsJumping;
	}

	private bool CanJumpCut()
	{
		return IsJumping && _rb.velocity.y > 0;
	}

	private int GetCurrentFrame(string animationName)
	{
		AnimatorClipInfo[] animationClip = _animationHandler.GetCurrentAnimatorClipInfo(0);

		int frame = 0;

		if (animationClip[0].clip.name == animationName)
		{
			frame = (int)(animationClip[0].weight * (animationClip[0].clip.length * animationClip[0].clip.frameRate));
		}

		return frame;
	}

	private void StartMovement(string[] animations)
	{
		AnimatorClipInfo[] animatorClipInfo = _animationHandler.GetCurrentAnimatorClipInfo(0);

		foreach (string animation in animations)
		{
			if (animatorClipInfo[0].clip.name == animation)
			{
				AnimatorStateInfo animatorStateInfo = _animationHandler.GetCurrentAnimatorStateInfo(0);

				Debug.Log(animatorClipInfo[0].clip.name + "  " + animatorStateInfo.normalizedTime);

				if (animatorStateInfo.normalizedTime >= 0.99f)
				{
					_stopMovement = false;
					return;
				}
			}
		}
	}

	#region Gizmos
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
	}
	#endregion
}
