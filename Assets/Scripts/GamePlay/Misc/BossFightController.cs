using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BossFightController : MonoBehaviour, IDataPersistener
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
	[SerializeField] private Animator _victoryScreen;

	[SerializeField] private LayerMask _playerMask;

	[HideInInspector]
	public DataSettings _dataSettings;

	private BoxCollider2D _trigger;

	private bool _bossIsDead;

	private bool _initBattle;
	private bool _battleEnded;

	private void Awake()
	{
		DataPersistenersManager.RegisterDataPersistener(this);
	}

	// Start is called before the first frame update
	void Start()
    {
		_trigger = GetComponent<BoxCollider2D>();

		_initBattle = true;
		_battleEnded = false;
    }

    // Update is called once per frame
    void Update()
    {
		if(_bossIsDead)
		{
			this.gameObject.SetActive(false);
			return;
		}

		// Set up the battle.
		if(_initBattle)
		{
			Collider2D playerCollider = Physics2D.OverlapBox(transform.position + (Vector3)_trigger.offset, _trigger.size, 0, _playerMask);

			if (playerCollider != null)
			{
				// battle is starting.
				_initBattle = false;
				
				// Close the battle zone.
				_doors.StartBoosFight();

				_player.PauseControl(3f);
				StartCoroutine(SummonBoss(1));
				StartCoroutine(StartMusic(3f));
			}
		}
		else if(_astaroth.gameObject.activeSelf) 
		{
			// Check if battle ends.
			if ((_player.IsDead || _astaroth.IsDead) && !_battleEnded)
			{
				EndBattle();

				if (_astaroth.IsDead)
				{
					_victoryScreen.SetTrigger("Play");
					_bossHealthBar.SetActive(false);
					_doors.EndBoosFight();
				}
			}
		}
    }

	private void EndBattle()
	{
		// Change battle loop clip to final clip.
		BackgroundMusicPlayer.Instance.PushClip(_end, false);

		_battleEnded = true;
		_bossIsDead = true;
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
		BackgroundMusicPlayer.Instance.PlayJustMusic();
		yield break;
	}

	public DataSettings GetDataSettings()
	{
		return _dataSettings;
	}

	public Data SaveData()
	{
		return new Data<bool>(_bossIsDead);
	}

	public void LoadData(Data data)
	{
		Data<bool> bossFightData = (Data<bool>)data;

		_bossIsDead = bossFightData.Value;
	}

	public void SetDataSettings(string dataTag, DataSettings.PersistenceType persistenceType)
	{
		_dataSettings.dataTag = dataTag;
		_dataSettings.persistenceType = persistenceType;
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(BossFightController))]
public class BossFightControllerEditor : DataPersisterEditor
{ }
#endif