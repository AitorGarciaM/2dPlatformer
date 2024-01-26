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

	[Header("Audio")]
	[SerializeField] private RandomAudioPlayer _footStepsPlayer;
	[SerializeField] private RandomAudioPlayer _hitPlayer;
	[SerializeField] private RandomAudioPlayer _healPlayer;

	[Header("HUD")]
	[SerializeField] private Slider _healthBar;
	[SerializeField] private Animator _deathScreen;
	[SerializeField] private TextMeshProUGUI _healCounter;
	[SerializeField] private Image _healIcon;
	[SerializeField] private GameObject _trinketsMenu;
	[SerializeField] private GameObject _pauseMenu;

	[Header("Layers & Tags")]
	[SerializeField] private LayerMask _enemiesLayer;
	[SerializeField] private string _cameraFocusTag;

	private Iinteractable _interactable;

	private Rigidbody2D _rb;
	private CurrentStats _currentStats;
	private MovementSystem _moveSystem;
	private PlayerAction _input = null;
	private PlayerInput _playerInput;

	private Transform _cameraFocus;
	private Vector2 _moveInputVector = Vector2.zero; // Input vector.
	private Vector2 _moveCamreraVector;
	private CheckPoint _resetPoint;

	private float _currentAttackWaitTime;
	private float _timeToEndAttack;
	private float _currentHitWaitTime = 1;
	private float _currentHealWaitTime = 0;

	private bool _jump;
	private bool _isFacingLeft;
	private bool _stopMovement;
	private bool _stopInput;
	private bool _deathScreenPlay;
	private bool _landFXInvoked = false;
	private bool _invincibility = false;

	public float LastPressedJumpTime { get; private set; }
	public float MovementSpeed { get { return _rb.velocity.x; } }
	public bool IsDead { get { return _currentStats.CurrentHealth <= 0; } }

	public CurrentStats GetStats()
	{
		return _currentStats;
	}

	public bool ResetingPosition
	{
		get; private set;
	}

	public void RestoreStats()
	{
		_currentStats.Init(_stats);
	}

	public void ResetHealth()
	{
		_currentStats.ResetHealth();

		_healCounter.gameObject.SetActive(true);
	}

	public void RestorePotions()
	{
		_currentStats.ResetHealings();
		_healCounter.text = _currentStats.CurrentHealings.ToString();
		_healIcon.color = new Color(1, 1, 1, 1);
	}

	public void SetLastTransitionDestination(CheckPoint transitionDestination)
	{
		_resetPoint = transitionDestination;
	}

	public void SetInteractable(Iinteractable iinteractable)
	{
		_interactable = iinteractable;
	}

	public void Hit(CurrentStats stats)
	{
		if (_currentHitWaitTime < _hitWaitTime || _invincibility) 
			return;

		_currentHitWaitTime = 0;
		CameraShaker.Instance.ShakeCamera(stats.ShakerForceImpact);
		//StartCoroutine(PauseInput(_hitWaitTime));
		_animationHandler.SetTrigger("Take_Damage");
		_currentStats.GetDamage(stats);
		_hitPlayer.PlayRandomSound();

		if (_currentStats.CurrentHealth <= 0)
		{
			_animationHandler.SetBool("Death", true);
			_stopMovement = true;
			_deathScreenPlay = true;
		}
	}

	public void Hit(float damage)
	{
		if (_currentHitWaitTime < _hitWaitTime || _invincibility)
			return;

		_currentHitWaitTime = 0;
		CameraShaker.Instance.ShakeCamera(_stats.ShakerForceImpact);
		StartCoroutine(PauseInput(_hitWaitTime));
		_animationHandler.SetTrigger("Take_Damage");
		_currentStats.GetDamage(damage);
		_hitPlayer.PlayRandomSound();

		if (_currentStats.CurrentHealth <= 0)
		{
			_animationHandler.SetBool("Death", true);
			_stopMovement = true;
			_deathScreenPlay = true;
		}
	}

	public void ResetPosition()
	{
		StartCoroutine(ResetPositionInternal());
		_stopMovement = false;
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
		//_input.Player.Disable();
		//_input.Disable();
		//_playerInput.DeactivateInput();
		_stopInput = true;
	}

	public void RestartControl()
	{
		////_input.Player.Enable();
		////_input.Enable();
		////_playerInput.ActivateInput();
		_stopInput = false;
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

	public void PlayFootSteps()
	{
		_footStepsPlayer.PlayRandomSound();
	}
	
	private void Awake()
	{
		_input = new PlayerAction();
		_currentStats = GetComponent<CurrentStats>();
		_playerInput = GetComponent<PlayerInput>();
		_rb = GetComponent<Rigidbody2D>();
		_moveSystem = GetComponent<MovementSystem>();
		
		foreach (Transform child in transform)
		{
			if(child.tag == _cameraFocusTag)
			{
				_cameraFocus = child;
			}
		}

		_isFacingLeft = false;
		_attackArea.gameObject.SetActive(false);
		_currentStats.Init(_stats);
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
		if (!_stopMovement || !_stopInput)
		{
			_moveInputVector = value.ReadValue<Vector2>();
		}
		else
		{
			_moveInputVector = Vector2.zero;
		}
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
		if (!_stopMovement || !_stopInput)
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
		if (!_stopInput)
		{
			_currentAttackWaitTime = 0;
			_timeToEndAttack = 0;
			_animationHandler.SetTrigger("Attack");
		}
	}

	private void OnHealInputOn()
	{
		if (_currentStats.CurrentHealings > 0 && _currentHealWaitTime >= 1f && !_stopInput)
		{
			_currentStats.Heal(_healing);

			if (_currentStats.CurrentHealth < _stats.BaseHealth)
			{
				StartCoroutine(PauseInput(0.7f));
				_animationHandler.SetTrigger("Heal");
				_currentStats.UseHealings();
				_healCounter.text = _currentStats.CurrentHealings.ToString();
				_currentHealWaitTime = 0;

				_healPlayer.PlayRandomSound();
			}
		}
	}

	private void OnInteractInputOn()
	{
		if (_interactable != null && !_stopInput)
		{
			_interactable.Interact();
		}
	}

	private void OnMenuInputOn()
	{
		if (!_stopInput)
		{
			_trinketsMenu.SetActive(!_trinketsMenu.activeSelf);

		}
	}

	private void OnPauseInputOn()
	{
		_pauseMenu.SetActive(!_pauseMenu.activeSelf);
	}

	#endregion

	private void Update()
	{
		if (_healCounter.text == "")
		{
			_healCounter.text = _currentStats.CurrentHealings.ToString();
		}
		else if (_currentStats.CurrentHealings == 0 && _healCounter.gameObject.activeSelf)
		{
			_healCounter.gameObject.SetActive(false);
			_healIcon.color = new Color(0.557f, 0.557f, 0.557f);
		}

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

		if (_currentStats.CurrentHealth <= 0)
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

		if(_moveSystem.IsLanding && !_landFXInvoked)
		{
			GameObject obj = Instantiate(_landFX, transform.position - (Vector3.up * transform.localScale.y / 2.5f), Quaternion.Euler(-90, 0, 0));
			_landFXInvoked = true;
			Destroy(obj, 1);
			Invoke("ResetLandFX", 1);
		}

		// Other Updates.
		_currentStats.Update();
	}

	private void ResetLandFX()
	{
		_landFXInvoked = false;
	}

	private void LateUpdate()
	{
		MoveCamera();

		_spriteRenderer.flipX = _isFacingLeft;

		// Updates health bar.
		_healthBar.normalizedValue = _currentStats.CurrentHealth / _stats.BaseHealth;
		
		// Reorientates attack position.
		if (_isFacingLeft)
		{
			_attackArea.transform.localPosition = new Vector2(-0.14f, _attackArea.transform.localPosition.y);
		}
		else
		{
			_attackArea.transform.localPosition = new Vector2(0.14f, _attackArea.transform.localPosition.y);
		}

		// Set animations parameters.
		if (!_stopInput)
		{
			_animationHandler.SetFloat("Velocity_X", Mathf.Abs(_moveInputVector.x));
			_animationHandler.SetFloat("Time_to_end_Attack", _timeToEndAttack);
		}
		else
		{
			_animationHandler.SetFloat("Velocity_X", 0);
		}

		_animationHandler.SetFloat("Velocity_Y", _rb.velocity.y);
		_animationHandler.SetBool("Is_Grounded", _moveSystem.IsGrounded);
		_animationHandler.SetBool("IsOnSlope", _moveSystem.IsOnSlope);
	}
	
	private void MoveCamera()
	{
		_cameraFocus.position = Vector2.MoveTowards(_cameraFocus.position, (Vector2)transform.position + _moveCamreraVector, 1 * Time.deltaTime);
		_cameraFocus.position = new Vector2(transform.position.x, _cameraFocus.position.y);

		
	}

	private void FixedUpdate()
	{
		Collider2D collider = Physics2D.OverlapBox(_attackArea.transform.position, _attackArea.size, 0, _enemiesLayer);

		_attackArea.gameObject.SetActive(_animationHandler.IsAttackActive);

		// Attack collision.
		if (collider != null && _attackArea.gameObject.activeSelf)
		{
			var hitable = collider.GetComponent<IHitable>();
			hitable.Hit(_currentStats);
			_currentStats.LiveSuctionOverDamage();
			_attackArea.gameObject.SetActive(false);
		}
		
		// Updates movement.
		if (_currentAttackWaitTime > _stats.AttackRate && !_stopInput)
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
		_stopInput = true;

		yield return new WaitForSeconds(time);

		_stopInput = false;

		yield break;
	}

	private IEnumerator DeathScreen()
	{
		_deathScreen.SetTrigger("Play");
		_deathScreenPlay = false;

		yield return new WaitForSeconds(2f);

		SceneController.Instance.TransitionToCheckPoint();

		yield break;
	}

	private IEnumerator ResetPositionInternal()
	{
		ResetingPosition = true;
		_invincibility = true;
		yield return ScreenFader.Instance.FadeSceneOut();
		_input.Disable();
		transform.position = new Vector3(_resetPoint.transform.position.x, _resetPoint.transform.position.y, transform.position.z);
		yield return ScreenFader.Instance.FadeSceneIn();
		_input.Enable();
		ResetingPosition = false;
		_invincibility = false;
		yield break;
	}
	
}
