using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class Stats : ScriptableObject, IDataPersistener
{
	[Header("Base Stats")]
	[SerializeField] private float _health;
	[SerializeField] private float _mana;
	[SerializeField] private float _healthRecover;
	[SerializeField] private float _manaRecover;
	[SerializeField] private float _attackRate;
	[SerializeField] private float _damage;
	
	public float BaseHealth { get { return _health; } }
	public float BaseMana { get { return _mana; } }
	public float HealthRecovery { get { return _healthRecover; } }
	public float ManaRecovery { get { return _manaRecover; } }
	public float Damage { get { return _damage; }}

	public float AttackRate { get { return _attackRate; } }

	public float CurrentHealth { get { return _currentHealth; } }
	public float CurrentMana { get { return _currentMana; } }

	public bool ResotreHealthOnSceneChange { get { return _currentHealth <= 0; } }

	private DataSettings _dataSettings;

	private HealingObject _healingObject;
	private float _elapsedHealingDuration;
	private float _currentHealth;
	private float _currentMana;
	
	public void Init()
	{
		_currentHealth = BaseHealth;
		_currentMana = BaseMana;
		_elapsedHealingDuration = 0;
		_dataSettings = new DataSettings(DataSettings.PersistenceType.ReadWrite);
		DataPersistenersManager.RegisterDataPersistener(this);
	}

	public void GetDamage(Stats agressorStates)
	{
		float damage = agressorStates.Damage;

		_currentHealth -= damage;
	}

	public void GetDamage(float damage)
	{
		_currentHealth -= damage;
	}

	public void Heal(HealingObject healingObject)
	{
		if (_elapsedHealingDuration <= 0)
		{
			_healingObject = healingObject;
			_elapsedHealingDuration = _healingObject.Duration;
		}
	}

	public void Update()
	{
		if((_currentHealth < _health) && (_elapsedHealingDuration > 0))
		{
			_currentHealth += _healingObject.HealthPerSecond * Time.deltaTime;
			Debug.Log(_elapsedHealingDuration);
		}
		else
		{
			_healingObject = null;
		}

		_elapsedHealingDuration -= Time.deltaTime;
	}

	public DataSettings GetDataSettings()
	{
		return _dataSettings;
	}

	public Data SaveData()
	{
		return new Data<float, float, bool>(_currentHealth, _currentMana, ResotreHealthOnSceneChange);
	}

	public void LoadData(Data data)
	{
		Data<float, float, bool> statsData = (Data<float,float,bool>)data;
		_currentHealth = statsData.Value2 ? BaseHealth : statsData.Value0;
		_currentMana = statsData.Value1;
	}
}
