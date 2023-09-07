using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MovementSystem))]
public class PlayerController : MonoBehaviour
{

	[Header("Attack")]
	[SerializeField] private float _attackWaitTime;

	[Header("Input")]
	[SerializeField] private float _jumpInputBufferTime;

	[Header("Visual Settings")]
	[SerializeField] private SpriteRenderer _spriteRenderer;
	[SerializeField] private Animator _animationHandler;
	[Space(10)]
	[SerializeField] private BoxCollider2D _attackArea;
	
	private Rigidbody2D _rb;
	private MovementSystem _moveSystem;
	private PlayerAction _input = null;
	private Vector2 _moveInputVector = Vector2.zero; // Input vector.

	[Header("Layers & Tags")]
	[SerializeField] private LayerMask _enemiesLayer;

	public float LastPressedJumpTime { get; private set; }
	public float MovementSpeed { get { return _rb.velocity.x; } }

	private float _currentAttackWaitTime;
	private float _timeToEndAttack;
	private bool _jump;
	private bool _isJumpCut;
	private bool _isFacingLeft;
	private bool _stopMovement;

	private void Awake()
	{
		_input = new PlayerAction();
		_rb = GetComponent<Rigidbody2D>();
		_moveSystem = GetComponent<MovementSystem>();

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
		_moveInputVector = value.ReadValue<Vector2>();
	}

	private void OnMovementCancelled(InputAction.CallbackContext value)
	{
		_moveInputVector = Vector2.zero;
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
		OnAttackInputOn();
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
		if (CanJumpCut())
		{
			_isJumpCut = true;
		}
	}

	private void OnAttackInputOn()
	{
		//_stopMovement = true;
		_currentAttackWaitTime = 0;
		_timeToEndAttack = 0;
		_animationHandler.SetTrigger("Attack");
		_attackArea.gameObject.SetActive(true);

		Collider2D collider = Physics2D.OverlapBox(_attackArea.transform.position, _attackArea.size, 0, _enemiesLayer);

		if (collider != null)
		{
			collider.GetComponent<SkeletoneController>().Hit();
		}
	}

	private void Update()
	{
		LastPressedJumpTime -= Time.deltaTime;
		_timeToEndAttack += Time.deltaTime;
		_currentAttackWaitTime += Time.deltaTime;

		if (_moveSystem.IsGrounded)
		{
			if (_moveSystem.LastOnGroundTime < -0.1f)
			{
				// Land animation.
			}
		}

		if (_currentAttackWaitTime > _attackWaitTime)
		{
			if (_moveInputVector.x > 0)
			{
				_isFacingLeft = false;
			}
			else if (_moveInputVector.x < 0)
			{
				_isFacingLeft = true;
			}
		}
		
		if (_moveSystem.CanJump() && LastPressedJumpTime > 0 && _currentAttackWaitTime > _attackWaitTime)
		{
			_isJumpCut = false;
			LastPressedJumpTime = 0;
			_moveSystem.Jump();
		}
	}

	private void LateUpdate()
	{
		_spriteRenderer.flipX = _isFacingLeft;
		int attackFrame = GetCurrentFrame("Attack");

		if (attackFrame == 4)
		{
			_attackArea.gameObject.SetActive(false);
		}

		//StartMovement(new string[] { "Attack", "Combo", "TakeDamage" });

		if (_isFacingLeft)
			_attackArea.transform.localPosition = new Vector2(-0.14f, _attackArea.transform.localPosition.y);
		else
			_attackArea.transform.localPosition = new Vector2(0.14f, _attackArea.transform.localPosition.y);

		_animationHandler.SetFloat("Velocity_X", Mathf.Abs(_rb.velocity.x));
		_animationHandler.SetFloat("Velocity_Y", _rb.velocity.y);
		_animationHandler.SetFloat("Time_to_end_Attack", _timeToEndAttack);
		_animationHandler.SetBool("IsGrounded", _moveSystem.IsGrounded);
	}

	private void FixedUpdate()
	{
		if (_currentAttackWaitTime > _attackWaitTime)
			_moveSystem.SetDesiredDirection(_moveInputVector);
		else
			_moveSystem.SetDesiredDirection(Vector2.zero);
	}

	private bool CanJumpCut()
	{
		return _moveSystem.IsJumping && _rb.velocity.y > 0;
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

				if (animatorStateInfo.normalizedTime > 1)
				{
					_stopMovement = false;
					Debug.Log("Movement Stoped");
					return;
				}
			}
		}
	}
}
