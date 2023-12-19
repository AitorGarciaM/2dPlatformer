using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(MovementSystem))]
public class PlayerController : MonoBehaviour, IHitable
{
	[Header("Stats")]
	[SerializeField] private Stats _stats;
	[SerializeField] private HealingObject _healing;
	[SerializeField] private int _healCount;

	[Header("Timers")]
	[SerializeField] private float _hitWaitTime;

	[Header("Input")]
	[SerializeField] private float _jumpInputBufferTime;

	[Header("Visual Settings")]
	[SerializeField] private SpriteRenderer _spriteRenderer;
	[SerializeField] private PlayerAnimationHandler _animationHandler;
	[SerializeField] private GameObject _jumpFX;
	[SerializeField] private GameObject _landFX;
	[Space(10)]
	[SerializeField] private BoxCollider2D _attackArea;

	[Header("HUD")]
	[SerializeField] private Slider _healthBar;
	[SerializeField] private Animator _deathScreen;
	[SerializeField] private TextMeshProUGUI _healCounter;
	[SerializeField] private Image _healIcon;
	[SerializeField] private GameObject _trinketsMenu;
	[SerializeField] private GameObject _pauseMenu;

	[Header("Layers & Tags")]
	[SerializeField] private LayerMask _enemiesLayer;

	private Iinteractable _interactable;

	private Rigidbody2D _rb;
	private MovementSystem _moveSystem;
	private PlayerAction _input = null;
	private Vector2 _moveInputVector = Vector2.zero; // Input vector.
	private Vector2 _moveCamreraVector;

	private float _currentAttackWaitTime;
	private float _timeToEndAttack;
	private float _currentHitWaitTime = 1;
	private float _currentHealWaitTime = 0;

	private int _currentHealthCount;

	private bool _jump;
	private bool _isFacingLeft;
	private bool _stopMovement;
	private bool _deathScreenPlay;

	public float LastPressedJumpTime { get; private set; }
	public float MovementSpeed { get { return _rb.velocity.x; } }
	public bool IsDead { get { return _stats.CurrentHealth <= 0; } }

	public Stats GetStats()
	{
		return _stats;
	}

	public void RestoreStats()
	{
		_stats.Init();
	}

	public void RestorePotions()
	{
		_currentHealthCount = _healCount;
	}

	public void SetInteractable(Iinteractable iinteractable)
	{
		_interactable = iinteractable;
	}

	public void Hit(Stats stats)
	{
		if (_currentHitWaitTime < _hitWaitTime)
			return;

		_currentHitWaitTime = 0;
		CameraShaker.Instance.ShakeCamera(stats.ShakerForceImpact);
		StartCoroutine(PauseInput(_hitWaitTime));
		_animationHandler.SetTrigger("Take_Damage");
		_stats.GetDamage(stats);

		if (_stats.CurrentHealth <= 0)
		{
			_animationHandler.SetBool("Death", true);
			_deathScreenPlay = true;
		}
	}

	public void Hit(float damage)
	{
		if (_currentHitWaitTime < _hitWaitTime)
			return;

		_currentHitWaitTime = 0;
		CameraShaker.Instance.ShakeCamera(_stats.ShakerForceImpact);
		StartCoroutine(PauseInput(_hitWaitTime));
		_animationHandler.SetTrigger("Take_Damage");
		_stats.GetDamage(damage);

		if (_stats.CurrentHealth <= 0)
		{
			_animationHandler.SetBool("Death", true);
			_deathScreenPlay = true;
		}
	}

	// Applies knokback to the player.
	public void Push(Vector2 direction, float force)
	{
		_rb.AddForce(direction * force, ForceMode2D.Impulse);
	}

	public void PauseControl(float time)
	{
		StartCoroutine(PauseInput(time));
	}

	public void PauseControl()
	{
		_input.Disable();
	}

	public void RestartControl()
	{
		_input.Enable();
	}

	public void SetForce(Vector2 force)
	{
		_rb.AddForce(force);
	}

	public void UpdateFacing(bool facingLeft)
	{
		_isFacingLeft = facingLeft;
	}

	public void TrasitionToNewScene()
	{
		_moveSystem.TransitionToNewScene();
	}

	private void Awake()
	{
		_input = new PlayerAction();
		_rb = GetComponent<Rigidbody2D>();
		_moveSystem = GetComponent<MovementSystem>();

		_isFacingLeft = false;
		_attackArea.gameObject.SetActive(false);
		_stats.Init();
		_currentHealthCount = _healCount;
		_healCounter.text = _currentHealthCount.ToString();
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
		_input.Player.Camera.performed += OnMoveCameraPerformed;
		_input.Player.Camera.canceled += OnMoveCameraCancelled;
		_input.Player.Heal.started += OnHealInputStarted;
		_input.Player.Interact.started += OnInteractStarted;
		_input.UI.TrinketMenu.started += OnMenuStarted;
		_input.UI.Pause.started += OnPauseStarted;
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
		_input.Player.Camera.performed -= OnMoveCameraPerformed;
		_input.Player.Camera.canceled -= OnMoveCameraCancelled;
		_input.Player.Heal.started -= OnHealInputStarted;
		_input.Player.Interact.started -= OnInteractStarted;
		_input.UI.TrinketMenu.started -= OnMenuStarted;
		_input.UI.Pause.started -= OnPauseStarted;
	}

	#region Input Detection

	private void OnMovementPerformed(InputAction.CallbackContext value)
	{
		_moveInputVector = value.ReadValue<Vector2>();
	}

	private void OnMovementCancelled(InputAction.CallbackContext value)
	{
		_moveInputVector = Vector2.zero;
	}

	private void OnMoveCameraPerformed(InputAction.CallbackContext context)
	{
		_moveCamreraVector = context.ReadValue<Vector2>();
	}

	private void OnMoveCameraCancelled(InputAction.CallbackContext context)
	{
		_moveCamreraVector = Vector2.zero;
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

	}

	private void OnHealInputStarted(InputAction.CallbackContext context)
	{
		OnHealInputOn();
	}

	private void OnInteractStarted(InputAction.CallbackContext context)
	{
		OnInteractInputOn();
	}

	private void OnMenuStarted(InputAction.CallbackContext context)
	{
		OnMenuInputOn();
	}

	private void OnPauseStarted(InputAction.CallbackContext context)
	{
		OnPauseInputOn();
	}

	// Jump performed;
	private void OnJumpingInput()
	{
		if (!_stopMovement)
		{
			LastPressedJumpTime = _jumpInputBufferTime;
			_animationHandler.SetTrigger("Jump");
		}
	}

	// Jump jumpCanceled.
	private void OnJumpingUpInput()
	{
		_moveSystem.OnStopJump();
	}

	private void OnAttackInputOn()
	{
		_currentAttackWaitTime = 0;
		_timeToEndAttack = 0;
		_animationHandler.SetTrigger("Attack");
	}

	private void OnHealInputOn()
	{
		if (_currentHealthCount > 0 && _currentHealWaitTime >= 1f)
		{
			_stats.Heal(_healing);

			if (_stats.CurrentHealth < _stats.BaseHealth)
			{
				StartCoroutine(PauseInput(0.7f));
				_animationHandler.SetTrigger("Heal");
				_currentHealthCount--;
				_healCounter.text = _currentHealthCount.ToString();
				_currentHealWaitTime = 0;

				if(_currentHealthCount == 0)
				{
					_healCounter.gameObject.SetActive(false);
					_healIcon.color = new Color(0.557f, 0.557f, 0.557f);
				}
			}
		}
	}

	private void OnInteractInputOn()
	{
		if (_interactable != null)
		{
			_interactable.Interact();
		}
	}

	private void OnMenuInputOn()
	{
		_trinketsMenu.SetActive(!_trinketsMenu.activeSelf);
	}

	private void OnPauseInputOn()
	{
		_pauseMenu.SetActive(!_pauseMenu.activeSelf);
	}

	#endregion

	private void Update()
	{
		if (_pauseMenu.activeSelf)
		{
			Time.timeScale = 0;
		}
		else
		{
			Time.timeScale = 1;
		}

		if (_trinketsMenu.activeSelf || _pauseMenu.activeSelf)
		{
			_input.Player.Disable();
		}
		else
		{
			_input.Player.Enable();
		}

		if (_stats.CurrentHealth <= 0)
		{
			if (_deathScreenPlay)
			{
				StartCoroutine(DeathScreen());
			}
			return;
		}

		// Timers.
		LastPressedJumpTime -= Time.deltaTime;
		_timeToEndAttack += Time.deltaTime;
		_currentAttackWaitTime += Time.deltaTime;
		_currentHitWaitTime += Time.deltaTime;
		_currentHealWaitTime += Time.deltaTime;

		// Reorientates player.
		if (_currentAttackWaitTime > _stats.AttackRate && Time.timeScale > 0)
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
		
		// Perform jump.
		if (_moveSystem.CanJump() && LastPressedJumpTime > 0 && _currentAttackWaitTime > _stats.AttackRate)
		{
			LastPressedJumpTime = 0;
			_moveSystem.Jump();
			GameObject obj = Instantiate(_jumpFX, transform.position - (Vector3.up * transform.localScale.y / 2.5f), Quaternion.Euler(-90, 0, 0));
			Destroy(obj, 1);
		}

		if(_moveSystem.IsLanding)
		{
			GameObject obj = Instantiate(_landFX, transform.position - (Vector3.up * transform.localScale.y / 2.5f), Quaternion.Euler(-90, 0, 0));
			Destroy(obj, 1);
		}

		// Other Updates.
		_stats.Update();
	}

	private void LateUpdate()
	{
		_spriteRenderer.flipX = _isFacingLeft;

		_attackArea.gameObject.SetActive(_animationHandler.IsAttackActive);

		// Updates health bar.
		_healthBar.normalizedValue = _stats.CurrentHealth / _stats.BaseHealth;

		// Reorientates attack position.
		if (_isFacingLeft)
		{
			_attackArea.transform.localPosition = new Vector2(-0.14f, _attackArea.transform.localPosition.y);
		}
		else
		{
			_attackArea.transform.localPosition = new Vector2(0.14f, _attackArea.transform.localPosition.y);
		}
		
		// Animations.
		_animationHandler.SetFloat("Velocity_X", Mathf.Abs(_rb.velocity.x));
		_animationHandler.SetFloat("Velocity_Y", _rb.velocity.y);
		_animationHandler.SetFloat("Time_to_end_Attack", _timeToEndAttack);
		_animationHandler.SetBool("Is_Grounded", _moveSystem.IsGrounded);
		_animationHandler.SetBool("IsOnSlope", _moveSystem.IsOnSlope);
	}

	private void FixedUpdate()
	{
		Collider2D collider = Physics2D.OverlapBox(_attackArea.transform.position, _attackArea.size, 0, _enemiesLayer);

		// Attack collision.
		if (collider != null && _attackArea.gameObject.activeSelf)
		{
			var hitable = collider.GetComponent<IHitable>();
			hitable.Hit(_stats);
			_stats.LiveSuctionOverDamage();
		}
		
		// Updates movement.
		if (_currentAttackWaitTime > _stats.AttackRate)
		{
			_moveSystem.SetDesiredDirection(_moveInputVector);
		}
		else
		{
			_moveSystem.SetDesiredDirection(Vector2.zero);
		}
	}

	private IEnumerator PauseInput(float time)
	{
		_input.Disable();

		yield return new WaitForSeconds(time);

		// Reactivate controll.
		_input.Enable();

		yield return null;
	}

	private IEnumerator DeathScreen()
	{
		_deathScreen.SetTrigger("Play");
		_deathScreenPlay = false;

		yield return new WaitForSeconds(2f);

		SceneController.Instance.TransitionToCheckPoint();

	}

	
}
