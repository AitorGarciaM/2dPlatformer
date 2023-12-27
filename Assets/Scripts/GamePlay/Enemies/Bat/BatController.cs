using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(MovementSystem), typeof(BatAnimator))]
public class BatController : MonoBehaviour, IHitable
{
	[SerializeField] private Stats _stats;
	[Space()]
	[SerializeField] private float _playerDetectRadius;
	[SerializeField] private float _playerAttackRadius;
	[SerializeField] private float _nextWaypointDistance;
	[Header("Physics")]
	[SerializeField] private CircleCollider2D _attackCollider;
	[Header("visual")]
	[SerializeField] private SpriteRenderer _spRenderer;
	[Header("Layers & Tags")]
	[SerializeField] private LayerMask _playerMask;

	private Rigidbody2D _rb;
	private BatAnimator _batAnimator;
	private MovementSystem _moveSystem;
	private Seeker _seeker;
	private PlayerController _player;
	private Vector2 _targetPosition;
	private Vector2 _startPosition;

	private Path _path;

	private int _currentWaypoint = 0;
	private bool _reachedEndOfPath = false;

	private bool _followPlayer;
	private bool _goBack;

	public Stats GetStats()
	{
		return _stats;
	}

	public void Hit(Stats stats)
	{
		_stats.GetDamage(stats);
		CameraShaker.Instance.ShakeCamera(stats.ShakerForceImpact);

		if(_stats.CurrentHealth <= 0)
		{
			_batAnimator.SetBool("Is_Death", true);
			_rb.gravityScale = 1;
			_moveSystem.enabled = false;
			this.enabled = false;
		}
		else
		{
			_batAnimator.SetTrigger("Hurt");
		}
	}

	public void Hit(float damage)
	{
		_stats.GetDamage(damage);
		
		if (_stats.CurrentHealth <= 0)
		{
			_batAnimator.SetBool("Is_Death", true);
			_rb.gravityScale = 1;
			_moveSystem.enabled = false;
			this.enabled = false;
		}
		else
		{
			_batAnimator.SetTrigger("Hurt");
		}
	}

	// Start is called before the first frame update
	void Start()
    {
		_rb = GetComponent<Rigidbody2D>();

		_batAnimator = GetComponent<BatAnimator>();
		_moveSystem = GetComponent<MovementSystem>();
		_seeker = GetComponent<Seeker>();

		_player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		_startPosition = transform.position;

		_targetPosition = _startPosition;

		_goBack = false;
		_followPlayer = false;

		_stats.Init();

		InvokeRepeating("UpdatePath", 0, 0.5f);

	}

	private void FixedUpdate()
	{
		// Check if player is on follow range.
		Collider2D player = Physics2D.OverlapCircle(transform.position, _playerDetectRadius, _playerMask);

		if(player != null)
		{
			_targetPosition = new Vector2(_player.transform.position.x, _player.transform.position.y - 0.048f);

			_followPlayer = true;
			_goBack = false;
		}
		else
		{
			_targetPosition = _startPosition;
			_followPlayer = false;
			_goBack = true;
		}

		// Check if player is in attack range.
		player = null;
		player = Physics2D.OverlapCircle(transform.position, _playerAttackRadius, _playerMask);

		if(player != null)
		{
			_batAnimator.SetTrigger("Attack");
		}

		// Apply damage to player.
		if(Physics2D.OverlapCircle(_attackCollider.transform.position, _attackCollider.radius, _playerMask) != null && _attackCollider.gameObject.activeSelf == true)
		{
			_player.Hit(_stats);
		}


		float distanceToStart = Vector2.Distance((Vector2)transform.position, _startPosition);

		if (distanceToStart < 0.1f)
		{
			_batAnimator.SetBool("Is_Sleep", true);
			_goBack = false;
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (_followPlayer || _goBack)
		{
			Move();
		}
	}

	private void LateUpdate()
	{
		if(_path == null)
		{
			return;
		}

		if (_currentWaypoint < _path.vectorPath.Count)
		{
			Vector2 direction = (_path.vectorPath[_currentWaypoint] - transform.position).normalized;
			if (direction.x < 0f)
			{
				_spRenderer.flipX = true;
			}
			else
			{
				_spRenderer.flipX = false;
			}
		}

		if(_followPlayer)
		{
			_batAnimator.SetBool("Is_Sleep", false);
		}
	}

	private void Move()
	{
		if (_path == null)
		{
			return;
		}

		if (_currentWaypoint >= _path.vectorPath.Count)
		{
			_reachedEndOfPath = true;
			return;
		}
		else
		{
			_reachedEndOfPath = false;
		}
		
		Vector2 direction = (_path.vectorPath[_currentWaypoint] - transform.position).normalized;

		Debug.DrawLine(transform.position, direction);

		_moveSystem.SetDesiredDirection(direction);

		float distance = Vector2.Distance(transform.position, _path.vectorPath[_currentWaypoint]);

		if (distance < _nextWaypointDistance)
		{
			_currentWaypoint++;
		}
	}

	#region Pathfinding
	private void UpdatePath()
	{
		if (_seeker.IsDone() && _goBack || _followPlayer)
		{
			_seeker.StartPath(transform.position, _targetPosition, OnPathComplete);
		}
	}

	private void OnPathComplete(Path p)
	{
		if(!p.error)
		{
			_path = p;
			_currentWaypoint = 0;
		}
	}
	#endregion


	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, _playerDetectRadius);

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, _playerAttackRadius);

		Gizmos.color = new Color(0.5f, 0, 1);
		Gizmos.DrawWireSphere(transform.position, _nextWaypointDistance);
	}

	
}
