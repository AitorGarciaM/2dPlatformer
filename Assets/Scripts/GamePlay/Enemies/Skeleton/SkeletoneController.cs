using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class SkeletoneController : MonoBehaviour, IHitable
{
	public enum State
	{
		Idle,
		Patroll,
		Follow,
		Attack,
		Dead
	}

	[Header("Stats")]
	[SerializeField] private Stats _stats;

	[Header("States")]
	[Space(5)]

	[Header("Patroll")]
	[SerializeField] private float _playerDetectionRadiurs;
	[SerializeField] private float _patrollWaitTime;
	[SerializeField] private List<Vector2> _patrollPoints = new List<Vector2>();

	[Header("Attack")]
	[SerializeField] private BoxCollider2D _attackArea;
	[SerializeField] private float _attackRange;

	[Header("Hit")]	
	[SerializeField] private float _hitWaitTime;
	[SerializeField] private float _hitDelayTime;
	
	[Space(5)]

	[Header("Visual")]
	[SerializeField] private SpriteRenderer _spRenderer;
	[SerializeField] private Animator _animationHandler;

	[Header("Audio")]
	[SerializeField] private RandomAudioPlayer _hitPlayer;

	[Space(5)]

	[Header("Layers & Tags")]
	[SerializeField] private LayerMask _playerLayerMask;

	private Rigidbody2D _rb;
	private CircleCollider2D _collider2d;
	private CurrentStats _currentStats;
	private MovementSystem _moveSystem;
	private Transform _target;
	private Seeker _seeker;
	private Path _path;

	private Vector2 _startPosition;
	private Vector2 _targetPosition;

	private float _direction;
	private float _currentPatrollWaitTime;
	private float _currentAttackWaitTime;
	private float _currentHitWaitTime;
	private float _currentHitDelayTime;
	private float _changeStateTimer;
	private float _directionSign;

	private State _state;

	private int _currentWayPoint;
	private int _currentPatrollPoint;
	private int _nextPatrollPoint;

	private bool _reachedEndOfPath = false;

	private bool _followPlayer;
	private bool _goBack;
	private bool _isFacingRight;
	private bool _isAttacking = false;
	private bool _reciveHit = false;

	public CurrentStats GetStats()
	{
		return _currentStats;
	}

	public void Hit(CurrentStats stats)
	{
		if (_currentHitWaitTime > _hitWaitTime)
		{
			_currentStats.GetDamage(stats);
			_reciveHit = true;
			_currentHitWaitTime = 0;
			_currentHitDelayTime = _hitDelayTime;
			_hitPlayer.PlayRandomSound();

			CameraShaker.Instance.ShakeCamera(stats.ShakerForceImpact);

			if (_currentStats.CurrentHealth > 0)
			{
				_state = State.Follow;
			}
			else
			{
				_state = State.Dead;
			}
		}
	}

	public void Hit(float damage)
	{
		if (_currentHitWaitTime > _hitWaitTime)
		{
			_currentStats.GetDamage(damage);
			_reciveHit = true;
			_currentHitWaitTime = 0;
			_currentHitDelayTime = _hitDelayTime;

			if (_currentStats.CurrentHealth > 0)
			{
				_state = State.Follow;
			}
			else
			{
				_state = State.Dead;
			}
		}
	}

	private void Awake()
	{
		_rb = GetComponent<Rigidbody2D>();
		_currentStats = GetComponent<CurrentStats>();
		_collider2d = GetComponent<CircleCollider2D>();
		_moveSystem = GetComponent<MovementSystem>();
		_seeker = GetComponent<Seeker>();

		_currentPatrollPoint = 0;

		for (int i = 1; i < _patrollPoints.Count; i++)
		{
			if (Vector2.Distance(_patrollPoints[_currentPatrollPoint], _rb.position) > Vector2.Distance(_patrollPoints[i], _rb.position))
			{
				_currentPatrollPoint = i;
			}
		}

		_nextPatrollPoint = (_currentPatrollPoint + 1) % _patrollPoints.Count;

		_state = State.Patroll;

		_currentHitDelayTime = _hitDelayTime;

		_currentStats.Init(_stats);
	}

	// Start is called before the first frame update
	void Start()
    {
		_targetPosition = _patrollPoints[_nextPatrollPoint];
		InvokeRepeating("UpdatePathfind", 0, 1f);
    }

	private void FixedUpdate()
	{
		
		//Check ground at desiredPosition.
		if (_direction != 0)
		{
			_directionSign = Mathf.Sign(_direction);
		}

		RaycastHit2D groundCheckRay = Physics2D.Raycast(new Vector2(transform.position.x + 0.25f * _directionSign, transform.position.y), Vector2.down, 0.25f, LayerMask.GetMask("Ground"));
		Debug.DrawLine(new Vector2(transform.position.x + 0.25f * Mathf.Sign(_direction), transform.position.y), new Vector2(transform.position.x + 0.25f * Mathf.Sign(_direction), transform.position.y) + Vector2.down * 0.25f);
		if (groundCheckRay.collider == null)
		{
			_reachedEndOfPath = true;
		}
		else
		{
			_reachedEndOfPath = false;
		}

		if (_attackArea.enabled == true)
		{
			Collider2D playercollider = Physics2D.OverlapBox(_attackArea.transform.position, _attackArea.size, _attackArea.transform.rotation.z, _playerLayerMask);
			
			if (playercollider != null)
			{
				playercollider.GetComponent<PlayerController>().Hit(_currentStats);
			}
		}

		if (_currentStats.CurrentHealth <= 0)
		{
			_state = State.Dead;
			_animationHandler.SetBool("IsDead", true);
		}
	}

	// Update is called once per frame
	void Update()
	{
		_currentAttackWaitTime += Time.deltaTime;
		_currentHitWaitTime += Time.deltaTime;
		_changeStateTimer -= Time.deltaTime;

		Debug.Log(_reachedEndOfPath);

		if (_reciveHit)
		{
			_currentHitDelayTime -= Time.deltaTime;
		}

		if(_currentHitWaitTime < _hitWaitTime)
		{
			_currentAttackWaitTime = _stats.AttackRate;
		}

		Collider2D collider = Physics2D.OverlapCircle(_rb.position, _playerDetectionRadiurs, _playerLayerMask);

		if (collider != null)
		{
			if (collider.transform.position.y > transform.position.y + 0.5f || collider.transform.position.y < transform.position.y - 0.5f)
			{
				collider = null;
			}
		}
		
		switch (_state)
		{
			case State.Idle:
				break;
			case State.Patroll:

				Patroll();

				_goBack = false;

				if(!_reachedEndOfPath)
				{
					if (collider != null && (collider.transform.position.y < _rb.position.y + 0.32f || collider.transform.position.y > _rb.position.y - 0.32f))
					{
						_state = State.Follow;
						_target = collider.transform;
					}
				}

				break;
			case State.Follow:

				if(_reachedEndOfPath || _target == null)
				{ 
					_state = State.Patroll;
					_target = null;
					_targetPosition = _patrollPoints[_nextPatrollPoint];
					_goBack = true;
					break;
				}
				else if (collider == null && _target != null)
				{
					_target = null;
					_state = State.Patroll;
					_targetPosition = _patrollPoints[_nextPatrollPoint];
					_goBack = true;
					break;
				}

				Follow();
				break;
			case State.Attack:

				if(_target == null)
				{
					_state = State.Patroll;
					_targetPosition = _patrollPoints[_nextPatrollPoint];
					_goBack = true;
					break;
				}

				_isAttacking = true;
				_direction = 0;

				if (Vector2.Distance(_rb.position, _target.position) > _attackRange && (_target.position.y > _rb.position.y + 0.32f || _target.position.y < _rb.position.y - 0.32f) || _currentAttackWaitTime < _stats.AttackRate)
				{
					_isAttacking = false;
					_state = State.Follow;
					break;
				}

				_currentAttackWaitTime = 0;
				_animationHandler.SetTrigger("Attack");
				
				break;
			case State.Dead:
				_attackArea.gameObject.SetActive(true);
				_attackArea.enabled = false;
				break;
			default:
				break;
		}

		UpdateDir();

		if (!_isAttacking)
		{
			Vector2 desiredDirection = new Vector2(_direction, 0);
			_moveSystem.SetDesiredDirection(desiredDirection);
		}
	}

	private void Follow()
	{
		if(Vector2.Distance(_rb.position, _target.position) > _attackRange)
		{
			_targetPosition = new Vector2(_target.position.x, transform.position.y);
			_followPlayer = true;
			_isAttacking = false;
		}
		else
		{
			_state = State.Attack;
			_direction = 0;
		}
	}

	private void Patroll()
	{
		_currentPatrollWaitTime += Time.deltaTime;

		float distanceToNextPoint = Vector2.Distance(_rb.position, _patrollPoints[_nextPatrollPoint]);

		if (distanceToNextPoint <= 0.1f && _currentPatrollWaitTime >= _patrollWaitTime)
		{
			_currentPatrollWaitTime = 0;
			_currentPatrollPoint = _nextPatrollPoint;
			_nextPatrollPoint = (_currentPatrollPoint + 1) % _patrollPoints.Count;
			_targetPosition = _patrollPoints[_nextPatrollPoint];
			_direction = 0;
		}
		else if (distanceToNextPoint <= 0.1f)
		{
			_targetPosition = transform.position;
		}
	}

	private void LateUpdate()
	{
		AnimatorClipInfo[] animatorClipInfo = _animationHandler.GetCurrentAnimatorClipInfo(0);

		if (animatorClipInfo[0].clip.name == "AttackLeft" || animatorClipInfo[0].clip.name == "AttackRight")
		{
			AnimatorStateInfo animatorStateInfo = _animationHandler.GetCurrentAnimatorStateInfo(0);

			if (animatorStateInfo.normalizedTime >= 1f)
			{
				_isAttacking = false;
				_currentAttackWaitTime = 0;
			}
		}

		if (_currentHitDelayTime <= 0)
		{
			if (_currentStats.CurrentHealth > 0)
			{
				_animationHandler.SetTrigger("Hit");
				_currentHitDelayTime = _hitDelayTime;
				_reciveHit = false;
			}
			else
			{
				_animationHandler.SetBool("IsDead", true);
				_rb.bodyType = RigidbodyType2D.Static;
				_collider2d.enabled = false;
				_state = State.Dead;
			}
		}

		_animationHandler.SetFloat("Velocity_X", Mathf.Abs(_rb.velocity.x));

		if (_rb.velocity.x > 0.001f)
		{
			_isFacingRight = false;
		}
		else if (_rb.velocity.x < -0.001f)
		{
			_isFacingRight = true;
		}
		else if(_target != null)
		{
			_isFacingRight = Mathf.Sign(_target.position.x - _rb.position.x) < 0;
		}

		_spRenderer.flipX = _isFacingRight;

		_animationHandler.SetBool("IsFacingRight", _isFacingRight);
	}

	#region Pathfinding

	private void UpdatePathfind()
	{
		if(_seeker.IsDone() || _goBack || _followPlayer)
		{
			_seeker.StartPath(transform.position, _targetPosition, OnPathComplete);
		}
	}

	private void OnPathComplete(Path p)
	{
		if(!p.error)
		{
			_path = p;
			_currentWayPoint = 0;
		}
		else
		{
			Debug.LogWarning(p.errorLog);
		}
	}

	private void UpdateDir()
	{
		if(_path == null || _currentPatrollWaitTime < _patrollWaitTime)
		{
			return;
		}

		if(_currentWayPoint >= _path.vectorPath.Count)
		{
			_direction = 0;
			return;
		}

		Vector2 dir = (_path.vectorPath[_currentWayPoint] - (Vector3)_rb.position).normalized;
		
		float distance = Vector2.Distance(_rb.position, _path.vectorPath[_currentWayPoint]);

		if (distance < 0.1f)
		{
			_currentWayPoint++;
		}
		else
		{
			if(dir.x != 0)
				_direction = Mathf.Sign(dir.x);
		}
	}

	#endregion

	#region Editor
	private void OnValidate()
	{
		for (int i = 0; i < _patrollPoints.Count; i++)
		{
			if (_patrollPoints[i].y == 0)
			{
				_patrollPoints[i] = new Vector2(_patrollPoints[i].x, transform.position.y);
			}
		}
	}

	#region Gizmos
	private void OnDrawGizmos()
	{
		
		Gizmos.color = Color.red;
		for (int i = 0; i < _patrollPoints.Count; i++)
		{
			Gizmos.DrawSphere(_patrollPoints[i], 0.03f);
		}

		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, _playerDetectionRadiurs);
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, _attackRange);
	}

	
	#endregion
	#endregion
}
