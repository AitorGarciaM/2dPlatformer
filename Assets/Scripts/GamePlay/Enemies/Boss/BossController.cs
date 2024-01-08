using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MovementSystem))]
public class BossController : MonoBehaviour, IHitable
{
	public enum Stage
	{
		BattleInit,
		First,
		Second,
	}

	[SerializeField] private Stats _stats;

	[Header("Ranges")]
	[SerializeField] private float _samshMaxRange;
	[SerializeField] private float _samshMinRange;
	[SerializeField] private float _fireFlameMinRange;

	[Header("Timers")]
	[SerializeField] private float _initBattleTime;
	[SerializeField] private float _smashCoolDown;
	[SerializeField] private float _jumpCooldDown;
	[SerializeField] private float _flameCoolDown;
	[SerializeField] private float _flameAttackTimer;
	[SerializeField] private float _hitWaitTime;
	[SerializeField] private float _flameHitPlayerTime;

	[Header("Systems")]
	[SerializeField] private BossAnimationHandler _animationHandler;
	[SerializeField] private MovementSystem _moveSystem;

	[Header("Physics")]
	[SerializeField] private CircleCollider2D _selfCollider;
	[SerializeField] private CapsuleCollider2D _flameCollider;
	[SerializeField] private BoxCollider2D _knifeCollider;
	[SerializeField] private BoxCollider2D _jumpCollider;
	[SerializeField] private BoxCollider2D _wallLeftCollider;
	[SerializeField] private BoxCollider2D _wallRightCollider;

	[Header("Forces")]
	[SerializeField] private float _knockBack = 5f;

	[Header("HUD")]
	[SerializeField] private Slider _healthBar;

	[Header("Visual")]
	[SerializeField] private SpriteRenderer _spRenderer;

	[Header("Layers")]
	[SerializeField] private LayerMask _playerMask;

	private PlayerController _player;

	private Rigidbody2D _rb;
	private GameObject _fireFlames;
	private Stage _stage;
	private float _currentInitBattleTime;
	private float _cooldDownTime;
	private float _wiatTime;
	private float _currentHitWaitTime;
	private float _currentFlameAttackTime;
	private float _currentFlameHitPlayerTime;
	private float _direction;
	private bool _startJump;
	private bool _performingJump;
	private bool _startingBattle;
	private bool _flamesHitPlayer;

	public bool IsDead { get { return (_stats.CurrentHealth <= 0); } }

	public Stats GetStats()
	{
		return _stats;
	}

	public void Hit(Stats stats)
	{
		if (_currentHitWaitTime < _hitWaitTime || _animationHandler.IsInvincible)
		{
			return;
		}

		CameraShaker.Instance.ShakeCamera(stats.ShakerForceImpact);
		_currentHitWaitTime = 0;
		_animationHandler.SetTrigger("Take_Damage");
		_stats.GetDamage(stats);

		_healthBar.normalizedValue = _stats.CurrentHealth / _stats.BaseHealth;
	}

	public void Hit(float damage)
	{
		throw new System.NotImplementedException();
	}

	void Start()
    {
		_rb = GetComponent<Rigidbody2D>();
		_player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		_stats.Init();
		_stage = Stage.BattleInit;
		_startingBattle = true;
		_fireFlames = transform.GetChild(1).gameObject;

		_currentFlameAttackTime = 0;
    }

	private void FixedUpdate()
	{
		// Flame attack Collision.
		if(_fireFlames.activeSelf)
		{
			Collider2D[] colliders = Physics2D.OverlapCapsuleAll(_flameCollider.transform.position, _flameCollider.size,_flameCollider.direction, _flameCollider.transform.rotation.eulerAngles.z, _playerMask);

			foreach (var collider in colliders)
			{
				if (collider != null && !_flamesHitPlayer)
				{
					float playerDir = Mathf.Sign(_player.transform.position.x - transform.position.x);
					Vector2 dir = new Vector2(playerDir, 0f);
					_player.Hit(_stats);
					_player.Push(dir, _knockBack);
					_flamesHitPlayer = true;
					_currentFlameHitPlayerTime = 0;
				}
			}
		}

		// Flame attack ends timer.
		if (_fireFlames.activeSelf)
		{
			if (_currentFlameAttackTime < _flameAttackTimer)
			{
				_currentFlameAttackTime += Time.deltaTime;
			}
			else
			{
				_animationHandler.SetTrigger("Flame_Attack_Ends");
				_currentFlameAttackTime = 0;
			}
		}

		// Smash attack Collision.
		if (_animationHandler.IsSmashing)
		{
			Collider2D playerCollider = Physics2D.OverlapBox(_knifeCollider.transform.position + new Vector3(_knifeCollider.offset.x, _knifeCollider.offset.y, 0), _knifeCollider.size, 0, _playerMask);

			if (playerCollider != null)
			{
				float playerDir = Mathf.Sign(_player.transform.position.x - transform.position.x);
				Vector2 dir = new Vector2(playerDir, 0f);
				_player.Hit(_stats);
				_player.Push(dir, _knockBack);
			}
		}

		// Jump Attack Collision.
		if(_animationHandler.IsLanding)
		{
			Collider2D playerCollider = Physics2D.OverlapBox(_jumpCollider.transform.position + new Vector3(_jumpCollider.offset.x, _jumpCollider.offset.y, 0), _jumpCollider.size, 0, _playerMask);

			if (playerCollider != null)
			{
				float playerDir = Mathf.Sign(_player.transform.position.x - transform.position.x);
				Vector2 dir = new Vector2(playerDir, 0f);
				_player.Hit(_stats);
				_player.Push(dir, _knockBack);
			}

			CameraShaker.Instance.ShakeCamera(0.1f);

			// Smash attack on Land.
			if (_stage == Stage.Second)
			{
				int prob = Random.Range(1, 101);

				if (prob < 10)
				{
					StartCoroutine(DelayedSmash(0.25f));
				}
			}
		}

		// Moves the entity towoards desired direction.
		Move();
	}

	void Update()
    {
		// Timers.
		_currentHitWaitTime += Time.deltaTime;
		_currentFlameHitPlayerTime += Time.deltaTime;

		// Reactivate flame attack collision with player after getting hitted.
		if(_currentFlameHitPlayerTime >= _flameHitPlayerTime)
		{
			_flamesHitPlayer = false;
		}

		switch (_stage)
		{
			case Stage.BattleInit:
				InitBattle();
				break;
			case Stage.First:
				FirstStage();
				break;
			case Stage.Second:
				SecondStage();
				break;
		}
	}

	private void LateUpdate()
	{
		float playerDir = Mathf.Sign(_player.transform.position.x - transform.position.x);

		// Orientates boss sprite towards player when it's not throwing flames.
		if (_animationHandler.IsFlipActive)
		{
			if (playerDir > 0)
			{
				_spRenderer.flipX = true;
			}
			else
			{
				_spRenderer.flipX = false;
			}
		}
		
		// Activates and deactivates flame gameobject acording to animator.
		if(_animationHandler.IsThrowingFlame)
		{
			_fireFlames.SetActive(true);
		}
		else
		{
			_fireFlames.SetActive(false);
		}

		// Activates and deactivates jump and knife collision acording to animator.
		_jumpCollider.enabled = _animationHandler.IsLanding;
		_knifeCollider.enabled = _animationHandler.IsSmashing;
		
		_animationHandler.SetBool("Is_Grounded", _moveSystem.IsGrounded);
		_animationHandler.SetFloat("Velocity_y", _rb.velocity.y);
	}

	private void InitBattle()
	{
		if(_moveSystem.IsGrounded && !_performingJump)
		{
			_currentInitBattleTime += Time.deltaTime;
		}
		
		if (_currentInitBattleTime >= _initBattleTime && _startingBattle)
		{
			Jump();
			_currentInitBattleTime = 0;
			_direction = Mathf.Sign(transform.position.x - _player.transform.position.x) * -1;
			Debug.Log(_direction);
		}
		
		if(_performingJump)
		{
			if(_startingBattle)
			{
				_startingBattle = false;
			}

			
		}
		else if(!_startingBattle && _moveSystem.IsGrounded)
		{
			_animationHandler.SetTrigger("Smash");
			_cooldDownTime = _smashCoolDown;
			_wiatTime = 0;
			_stage = Stage.First;
		}
	}

	private void FirstStage()
	{
		if (AttackCoolDown())
		{
			BaseAttackPattern();
		}

		if (_stats.CurrentHealth <= (_stats.BaseHealth * 0.7f))
		{
			_stage = Stage.Second;
		}
	}

	private void SecondStage()
	{
		if(AttackCoolDown())
		{
			float wallRightDistance = Mathf.Abs(transform.position.x - _wallRightCollider.transform.position.x);
			float wallLeftDistance = Mathf.Abs(transform.position.x - _wallLeftCollider.transform.position.x);

			float playerDistance = Mathf.Abs(transform.position.x - _player.transform.position.x);
			
			if (wallRightDistance < _samshMaxRange)
			{
				// if boss is too close of the right wall jumps away from it.
				_direction = 1;
				Jump();
			}
			else if (wallLeftDistance < _samshMaxRange)
			{
				// if boss is too close of the left wall jumps away from it.
				_direction = -1;
				Jump();
			}
			else if (playerDistance < _fireFlameMinRange)
			{
				// if player is too close to the player jumps away of the player.
				if (wallRightDistance < _samshMaxRange)
				{
					_direction = 1;
				}
				else if (wallLeftDistance < _samshMaxRange)
				{
					_direction = -1;
				}
				else
				{
					_direction = Mathf.Sign(_player.transform.position.x - transform.position.x) * -1;
				}
					
				Jump();
			}
			else
			{
				if (playerDistance < _samshMinRange)
				{
					ThrowingFlame();
				}
				else
				{
					BaseAttackPattern();
				}
			}
		}

		if (IsDead)
		{
			// Play Death animation.
			_animationHandler.SetTrigger("Is_Death");

			// Deactivates boss behaivour and physics.
			this.enabled = false;
			_rb.bodyType = RigidbodyType2D.Static;
			_selfCollider.enabled = false;
		}
	}

	private void Jump()
	{
		_moveSystem.Jump();
		_cooldDownTime = _jumpCooldDown;
		_startJump = true;
		_animationHandler.SetTrigger("Jump_Attack");
	}

	private void ThrowingFlame()
	{
		_animationHandler.SetTrigger("Flame_Attack");
		
		if(_spRenderer.flipX)
		{
			_fireFlames.transform.rotation = Quaternion.Euler(0, 0, 140f);
			_fireFlames.transform.localPosition = new Vector3(Mathf.Abs(_fireFlames.transform.localPosition.x), _fireFlames.transform.localPosition.y, _fireFlames.transform.localPosition.z);
		}
		else
		{
			_fireFlames.transform.rotation = Quaternion.Euler(0, 0, 40f);
			_fireFlames.transform.localPosition = new Vector3(Mathf.Abs(_fireFlames.transform.localPosition.x) * -1, _fireFlames.transform.localPosition.y, _fireFlames.transform.localPosition.z);
		}
	}

	private void Move()
	{
		if(_startJump)
		{
			_performingJump = true;
		}

		if(!_startJump && _moveSystem.IsGrounded)
		{
			_performingJump = false;
		}

		if (_performingJump)
		{
			_startJump = false;
			_moveSystem.SetDesiredDirection(Vector2.left * _direction);
		}
		else
		{
			_direction = 0;
		}
	}

	private bool AttackCoolDown()
	{
		if (_animationHandler.IsAttackEnding)
		{
			_wiatTime += Time.deltaTime;
		}
		else
		{
			_wiatTime = 0;
		}

		if (!_animationHandler.IsAttackEnding || (_wiatTime <= _cooldDownTime))
		{
			return false;
		}

		_wiatTime = 0;
		return true;
	}

	private void Smash()
	{
		_animationHandler.SetTrigger("Smash");
		_cooldDownTime = _smashCoolDown;

		if(_spRenderer.flipX)
		{
			_knifeCollider.offset = new Vector2(Mathf.Abs(_knifeCollider.offset.x), _knifeCollider.offset.y);
		}
		else
		{
			_knifeCollider.offset = new Vector2(Mathf.Abs(_knifeCollider.offset.x) * -1, _knifeCollider.offset.y);
		}
	}

	private void BaseAttackPattern()
	{
		// Select the attack.
		float distance = Mathf.Abs(transform.position.x - _player.transform.position.x);

		if ((distance < _samshMaxRange && distance > _samshMinRange))
		{
			Smash();
		}
		else
		{
			Jump();

			_direction = Mathf.Sign(transform.position.x - _player.transform.position.x);
		}
	}

	private IEnumerator EndFlameAttack()
	{
		yield return new WaitForSeconds(_flameAttackTimer);

		_fireFlames.SetActive(false);

		yield return null;
	}

	private IEnumerator DelayedSmash(float time)
	{
		yield return new WaitForSeconds(time);
		Smash();
		yield return null;
	}

	#region Gizmos
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Vector3 smashCircleCenter = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
		Gizmos.DrawWireSphere(smashCircleCenter, _samshMaxRange);
		Gizmos.DrawWireSphere(smashCircleCenter, _samshMinRange);
		Gizmos.DrawWireSphere(smashCircleCenter, _fireFlameMinRange);
	}
	#endregion
}
