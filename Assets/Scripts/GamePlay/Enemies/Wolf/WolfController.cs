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
	[Header("Layers & tags")]
	[SerializeField] private LayerMask _playerMask;
	[SerializeField] private LayerMask _groundMask;

	private PlayerController _player;

	private float _currentInvincibleTime;
	private float _currentAttackRateTime;
	private float _direction;
	private bool _changeDir;
	private bool _attack;

	public Stats GetStats()
	{
		return _stats;
	}

	public bool IsDeath { get; private set; }

	public void Hit(Stats stats)
	{
		if(_currentInvincibleTime < _invincibleCooldown)
		{
			return;
		}

		_stats.GetDamage(stats);

		_animationHandler.SetTrigger("Hit");

		_currentInvincibleTime = 0;
		_currentAttackRateTime = 0;
		_attack = false;

		CameraShaker.Instance.ShakeCamera(stats.ShakerForceImpact);

		if(_stats.CurrentHealth <= 0)
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

		_stats.GetDamage(damage);

		_animationHandler.SetTrigger("Hit");

		_currentInvincibleTime = 0;
		_currentAttackRateTime = 0;
		_attack = false;

		if (_stats.CurrentHealth <= 0)
		{
			IsDeath = true;
			_animationHandler.SetBool("Is_Death", IsDeath);
			this.enabled = false;
		}
	}

	// Start is called before the first frame update
	void Start()
    {
		_player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

		IsDeath = false;
		_stats.Init();
    }

	private void FixedUpdate()
	{
		// Detects if player is attack range.
		if (Physics2D.OverlapCircle(_attackArea.transform.position, _attackPlayerDistance, _playerMask))
		{
			if (_currentAttackRateTime >= _stats.AttackRate)
			{
				_attack = true;
				_currentAttackRateTime = 0;
			}
		}

		// Inflicts damage to the player.
		if (Physics2D.OverlapCircle(_attackArea.transform.position, _attackArea.radius, _playerMask))
		{
			if(_attackArea.enabled == true)
			{
				_player.Hit(_stats);
			}
		}

		// Check if is colliding with a wall.
		if (Physics2D.OverlapBox(_wallChecker.transform.position, _wallChecker.size, 0, _groundMask))
		{
			Debug.Log("Colliding wall");
			_direction *= -1;
		}
	}

	// Update is called once per frame
	void Update()
    {
		#region Timers
		_currentInvincibleTime += Time.deltaTime;
		_currentAttackRateTime += Time.deltaTime;
		#endregion

		// Changes direction when de wolf has passed de player.
		if (transform.position.x >= _player.transform.position.x + 1 || transform.position.x <= _player.transform.position.x - 1)
		{
			_changeDir = true;
		}
		
		if (_animationHandler.CanMove && _changeDir)
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
			_attack = false;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(_attackArea.transform.position, _attackPlayerDistance);
	}
}
