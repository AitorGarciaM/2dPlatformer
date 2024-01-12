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

	[Space(5)]

	[Header("Layers & Tags")]
	[SerializeField] private LayerMask _playerLayerMask;

	private Rigidbody2D _rb;
	private CircleCollider2D _collider2d;
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

	public Stats GetStats()
	{
		return _stats;
	}

	public void Hit(Stats stats)
	{
		if (_currentHitWaitTime > _hitWaitTime)
		{
			_stats.GetDamage(stats);
			_reciveHit = true;
			_currentHitWaitTime = 0;
			_currentHitDelayTime = _hitDelayTime;

			CameraShaker.Instance.ShakeCamera(stats.ShakerForceImpact);

			if (_stats.CurrentHealth > 0)
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
			_stats.GetDamage(damage);
			_reciveHit = true;
			_currentHitWaitTime = 0;
			_currentHitDelayTime = _hitDelayTime;

			if (_stats.CurrentHealth > 0)
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

		_stats.Init();
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
		RaycastHit2D groundCheckRay = Physics2D.Raycast(new Vector2(transform.position.x + 0.25f * Mathf.Sign(_direction), transform.position.y), Vector2.down, 0.25f, LayerMask.GetMask("Ground"));

		if(groundCheckRay.collider == null)
		{
			_state = State.Patroll;
			_changeStateTimer = 2f;
		}

		if (_attackArea.enabled == true)
		{
			Collider2D playercollider = Physics2D.OverlapBox(_attackArea.transform.position, _attackArea.size, _attackArea.transform.rotation.z, _playerLayerMask);

			if (playercollider != null)
			{
				playercollider.GetComponent<PlayerController>().Hit(_stats);
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		_currentAttackWaitTime += Time.deltaTime;
		_currentHitWaitTime += Time.deltaTime;
		_changeStateTimer -= Time.deltaTime;

		if (_reciveHit)
		{
			_currentHitDelayTime -= Time.deltaTime;
		}

		if(_currentHitWaitTime < _hitWaitTime)
		{
			_currentAttackWaitTime = _stats.AttackRate;
		}

		Collider2D collider = Physics2D.OverlapCircle(_rb.position, _playerDetectionRadiurs, _playerLayerMask);
		
		switch (_state)
		{
			case State.Idle:
				break;
			case State.Patroll:

				Patroll();

				_goBack = false;

				if (collider != null && _changeStateTimer <= 0)
				{
					_state = State.Follow;
					_target = collider.transform;
				}

				break;
			case State.Follow:
				
				Follow();
				
				if (collider == null && _target != null)
				{
					_target = null;
					_targetPosition = _patrollPoints[_nextPatrollPoint];
					_goBack = true;
					_state = State.Patroll;
				}

				break;
			case State.Attack:

				_isAttacking = true;
				_direction = 0;

				if (Vector2.Distance(_rb.position, _target.position) > _attackRange || _currentAttackWaitTime < _stats.AttackRate)
				{
					_state = State.Follow;
					break;
				}

				_currentAttackWaitTime = 0;
				_animationHandler.SetTrigger("Attack");
				
				break;
			case State.Dead:
				// Do Nothign.
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
			_targetPosition = _target.position;
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
			if (_stats.CurrentHealth > 0)
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
		_spRenderer.flipX = _isFacingRight;

		if (!_isAttacking)
		{
			if (Mathf.Sign(_rb.velocity.x) > 0)
				_isFacingRight = false;
			else if(Mathf.Sign(_rb.velocity.x) < 0)
				_isFacingRight = true;
		}

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
			_reachedEndOfPath = true;
			_direction = 0;
			return;
		}
		else
		{
			_reachedEndOfPath = false;
		}

		Vector2 dir = (_path.vectorPath[_currentWayPoint] - (Vector3)_rb.position).normalized;
		
		Debug.DrawLine(_rb.position, dir);

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
