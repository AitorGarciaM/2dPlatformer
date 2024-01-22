using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovementSystem))]
public class WolfController : MonoBehaviour, IHitable
{
	[SerializeField] private Stats _stats;
	[Header("Physics")]
	[SerializeField] private CircleCollider2D _attackArea;
	[SerializeField] private BoxCollider2D _wallChecker;
	[SerializeField] private float _attackPlayerDistance;
	[SerializeField] private float _invincibleCooldown;
	[Header("Systems")]
	[SerializeField] private MovementSystem _movementSystem;
	[SerializeField] private WolfAnimationHandler _animationHandler;
	[SerializeField] private SpriteRenderer _spRenderer;
	[Header("Audio")]
	[SerializeField] private RandomAudioPlayer _attackPlayer;
	[SerializeField] private RandomAudioPlayer _hitPlayer;
	[Header("Layers & tags")]
	[SerializeField] private LayerMask _playerMask;
	[SerializeField] private LayerMask _groundMask;

	private PlayerController _player;
	private CurrentStats _currentStats;

	private float _currentInvincibleTime;
	private float _currentAttackRateTime;
	private float _currentObstacleFoundTime;
	private float _currentAttackDeactivationTime = 0;
	private float _direction;
	private bool _changeDir;
	private bool _attack;
	private bool _obstacleFound = false;

	public CurrentStats GetStats()
	{
		return _currentStats;
	}

	public bool IsDeath { get; private set; }

	public void Hit(CurrentStats stats)
	{
		if(_currentInvincibleTime < _invincibleCooldown)
		{
			return;
		}

		_currentStats.GetDamage(stats);

		_animationHandler.SetTrigger("Hit");
		_hitPlayer.PlayRandomSound();

		_currentInvincibleTime = 0;
		_currentAttackRateTime = 0;
		_currentAttackDeactivationTime = 10;
		_attack = false;


		CameraShaker.Instance.ShakeCamera(stats.ShakerForceImpact);

		if(_currentStats.CurrentHealth <= 0)
		{
			IsDeath = true;
			_animationHandler.SetBool("Is_Death", IsDeath);
			this.enabled = false;
		}
	}

	public void Hit(float damage)
	{
		if (_currentInvincibleTime < _invincibleCooldown)
		{
			return;
		}

		_currentStats.GetDamage(damage);

		_animationHandler.SetTrigger("Hit");

		_currentInvincibleTime = 0;
		_currentAttackRateTime = 0;
		_attack = false;

		if (_currentStats.CurrentHealth <= 0)
		{
			IsDeath = true;
			_animationHandler.SetBool("Is_Death", IsDeath);
			this.enabled = false;
		}
	}

	// Start is called before the first frame update
	void Start()
    {
		_currentStats = GetComponent<CurrentStats>();
		_player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

		IsDeath = false;
		_currentStats.Init(_stats);
    }

	private void FixedUpdate()
	{
		_obstacleFound = false;

		// Detects if there is ground infront.
		int dir = _spRenderer.flipX ? 1 : -1;

		RaycastHit2D groundCheckRay = Physics2D.Raycast(new Vector2(transform.position.x + 0.25f * dir, transform.position.y), Vector2.down, 0.25f, LayerMask.GetMask("Ground"));
		Debug.DrawLine(new Vector3(transform.position.x + 0.25f * dir, transform.position.y), new Vector3(transform.position.x + 0.25f * dir, transform.position.y - 0.25f), Color.red);

		if(groundCheckRay.collider == null)
		{
			// Change direction.
			_direction *= -1;
			_currentObstacleFoundTime = 2f;
			_obstacleFound = true;
			Debug.LogWarning("Wolf can't reach player.");
		}

		// Detects if player is attack range.
		if (Physics2D.OverlapCircle(_attackArea.transform.position, _attackPlayerDistance, _playerMask))
		{
			if (_currentAttackRateTime >= _stats.AttackRate && _currentAttackDeactivationTime <= 0)
			{
				_attack = true;
				_currentAttackRateTime = 0;
				_attackArea.enabled = true;
			}
			else if(_currentAttackDeactivationTime >= 0)
			{
				_attackArea.enabled = false;
			}
		}

		// Inflicts damage to the player.
		if (Physics2D.OverlapCircle(_attackArea.transform.position, _attackArea.radius, _playerMask))
		{
			if(_attackArea.enabled == true)
			{
				_player.Hit(_currentStats);
			}
		}

		// Check if is colliding with a wall.
		if (Physics2D.OverlapBox(_wallChecker.transform.position, _wallChecker.size, 0, _groundMask))
		{
			_direction *= -1;
			_obstacleFound = true;
		}
	}

	// Update is called once per frame
	void Update()
    {
		#region Timers
		_currentInvincibleTime += Time.deltaTime;
		_currentAttackRateTime += Time.deltaTime;
		_currentObstacleFoundTime -= Time.deltaTime;
		_currentAttackDeactivationTime -= Time.deltaTime;
		#endregion

		// Changes direction when de wolf has passed de player.
		if (transform.position.x >= _player.transform.position.x + 1 || transform.position.x <= _player.transform.position.x - 1)
		{
			_changeDir = true;
		}
		
		if (_animationHandler.CanMove && _changeDir && (_currentObstacleFoundTime <= 0))
		{
			_direction = Mathf.Sign(_player.transform.position.x - transform.position.x);
			_changeDir = false;
		}
		else if(!_animationHandler.CanMove)
		{
			_direction = 0;
		}

		if(_direction > 0)
		{
			_spRenderer.flipX = true;
			_attackArea.transform.localPosition = new Vector3(Mathf.Abs(_attackArea.transform.localPosition.x), _attackArea.transform.localPosition.y, _attackArea.transform.localPosition.z);
			_wallChecker.transform.localPosition = new Vector3(Mathf.Abs(_wallChecker.transform.localPosition.x), _wallChecker.transform.localPosition.y, _wallChecker.transform.localPosition.z);
		}
		else
		{
			_spRenderer.flipX = false;
			_attackArea.transform.localPosition = new Vector3(Mathf.Abs(_attackArea.transform.localPosition.x) * -1, _attackArea.transform.localPosition.y, _attackArea.transform.localPosition.z);
			_wallChecker.transform.localPosition = new Vector3(Mathf.Abs(_wallChecker.transform.localPosition.x) * -1, _wallChecker.transform.localPosition.y, _wallChecker.transform.localPosition.z);
		}

		_movementSystem.SetDesiredDirection(new Vector2(_direction, 0));
    }

	private void LateUpdate()
	{
		if(_attack)
		{
			_animationHandler.SetTrigger("Attack");
			_attackPlayer.PlayRandomSound();
			_attack = false;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(_attackArea.transform.position, _attackPlayerDistance);
	}
}
