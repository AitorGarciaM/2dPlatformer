using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFightController : MonoBehaviour
{
	[Header("HUD")]
	[SerializeField] private GameObject _bossHealthBar;

	[Header("Objects")]
	[SerializeField] private Doors _doors;
	[SerializeField] private  BossController _astaroth;
	[SerializeField] private PlayerController _player;

	[Header("Audios")]
	[SerializeField] private AudioClip _loop;
	[SerializeField] private AudioClip _end;

	[Header("Systems")]
	[SerializeField] private AudioSource _musicSource;
	[SerializeField] private Animator _deathScreen;

	[SerializeField] private LayerMask _playerMask;

	private BoxCollider2D _trigger;

	private bool _initBattle;
	private bool _endBattle;

    // Start is called before the first frame update
    void Start()
    {
		_trigger = GetComponent<BoxCollider2D>();

		_initBattle = true;
		_endBattle = false;
		_musicSource.clip = _loop;
		_musicSource.loop = true;
    }

    // Update is called once per frame
    void Update()
    {
		if(_initBattle)
		{
			Collider2D playerCollider = Physics2D.OverlapBox(transform.position + (Vector3)_trigger.offset, _trigger.size, 0, _playerMask);

			if (playerCollider != null)
			{
				_initBattle = false;

				_doors.StartBoosFight();
				_player.PauseControll(3f);
				StartCoroutine(SummonBoss(1));
				StartCoroutine(StartMusic(3f));
			}
		}

		if((_player.IsDead || _astaroth.IsDead) && !_endBattle)
		{
			EndBattle();
		}

		if(_player.IsDead)
		{
			_deathScreen.SetTrigger("Play");
		}
    }

	private void EndBattle()
	{
		_musicSource.Pause();
		_musicSource.clip = _end;
		_musicSource.loop = false;
		_musicSource.Play();
		_endBattle = true;
	}

	private IEnumerator SummonBoss(float time)
	{
		yield return new WaitForSeconds(time);
		_astaroth.gameObject.SetActive(true);
		_bossHealthBar.SetActive(true);
		yield return null;
	}

	private IEnumerator StartMusic(float time)
	{
		yield return new WaitForSeconds(time);
		_musicSource.Play();
		yield return null;
	}
}
