using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(MovementSystem))]
public class PlayerController : MonoBehaviour
{

	[Header("Stats")]
	[SerializeField] private Stats _stats;

	[Header("Timers")]
	[SerializeField] private float _hitWaitTime;

	[Header("Input")]
	[SerializeField] private float _jumpInputBufferTime;

	[Header("Visual Settings")]
	[SerializeField] private SpriteRenderer _spriteRenderer;
	[SerializeField] private Animator _animationHandler;
	[Space(10)]
	[SerializeField] private BoxCollider2D _attackArea;

	[Header("HUD")]
	[SerializeField] Slider _healthBar;
	
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
	private float _currentHitWaitTime;

	private bool _jump;
	private bool _isJumpCut;
	private bool _isFacingLeft;
	private bool _stopMovement;
	private bool _attackIsPerforming = false;
	private bool _deactiveMovement;

	public void Hit(Stats stats)
	{
		if (_currentHitWaitTime < _hitWaitTime)
			return;

		_currentHitWaitTime = 0;
		_deactiveMovement = true;
		_animationHandler.SetTrigger("TakeDamage");
		_stats.GetDamage(stats);

		_healthBar.normalizedValue = _stats.CurrentHealth / _stats.BaseHealth;

		if (_stats.CurrentHealth <= 0)
			_animationHandler.SetBool("Death", true);
	}

	private void Awake()
	{
		_input = new PlayerAction();
		_rb = GetComponent<Rigidbody2D>();
		_moveSystem = GetComponent<MovementSystem>();

		_isFacingLeft = false;
		_attackArea.gameObject.SetActive(false);
		_stats.Init();
	}

	private void OnEnable()
	{
		_input.Enable();
		_input.Player.Movement.performed += OnMovementPerformed;
		_input.Player.Movement.canceled += OnMovementCancelled;
		_input.Player.Jump.started += OnJumpStarted;
		_input.Player.Jump.canceled += OnJumpCanceled;
		_input.Player.Attack.started += OnAttackStarted;
		_input.Player.Attack.canceled += OnAttackCanceled;
	}

	private void OnDisable()
	{
		_input.Disable();
		_input.Player.Movement.performed -= OnMovementPerformed;
		_input.Player.Movement.canceled -= OnMovementCancelled;
		_input.Player.Jump.started -= OnJumpStarted;
		_input.Player.Jump.canceled -= OnJumpCanceled;
		_input.Player.Attack.started -= OnAttackStarted;
		_input.Player.Attack.canceled -= OnAttackCanceled;
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

	public void OnAttackCanceled(InputAction.CallbackContext context)
	{
		_attackIsPerforming = false;
	}

	#endregion

	// Jump performed;
	private void OnJumpingInput()
	{
		LastPressedJumpTime = _jumpInputBufferTime;
		_animationHandler.SetTrigger("Jump");
	}

	// Jump jumpCanceled.
	private void OnJumpingUpInput()
	{
		if (CanJumpCut())
		{
			_isJumpCut = true;
		}
	}

	private void OnAttackInputOn()
	{
		_currentAttackWaitTime = 0;
		_timeToEndAttack = 0;
		_animationHandler.SetTrigger("Attack");
		_attackArea.gameObject.SetActive(true);

		Collider2D collider = Physics2D.OverlapBox(_attackArea.transform.position, _attackArea.size, 0, _enemiesLayer);

		if (collider != null && !_attackIsPerforming)
		{
			collider.GetComponent<SkeletoneController>().Hit(_stats);
		}

		_attackIsPerforming = true;
	}

	private void Update()
	{
		if (_stats.CurrentHealth <= 0)
			return;

		LastPressedJumpTime -= Time.deltaTime;
		_timeToEndAttack += Time.deltaTime;
		_currentAttackWaitTime += Time.deltaTime;
		_currentHitWaitTime += Time.deltaTime;

		if(_currentHitWaitTime > _hitWaitTime * 0.5f)
		{
			_deactiveMovement = false;
		}
			
		if(_deactiveMovement)
		{
			return;
		}

		if (_currentAttackWaitTime > _stats.AttackRate)
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
		
		if (_moveSystem.CanJump() && LastPressedJumpTime > 0 && _currentAttackWaitTime > _stats.AttackRate)
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

		if(attackFrame == 3)
		{
			_attackArea.gameObject.SetActive(true);
		}
		else if (attackFrame == 4)
		{
			_attackArea.gameObject.SetActive(false);
		}
		
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
		if (_stats.CurrentHealth <= 0 || _deactiveMovement)
			return;

		if (_currentAttackWaitTime > _stats.AttackRate)
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
