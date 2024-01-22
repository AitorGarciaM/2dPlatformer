using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CurrentStats : MonoBehaviour, IDataPersistener
{
	private Stats _baseStats;

	private float _currentHealth;
	private int _currentHealings;
	private float _currentMana;
	private float _currentDamage;
	private float _currentHealthRegeneration;
	private float _currentLiveSuction;

	public float CurrentHealth { get { return _currentHealth; }}
	public int CurrentHealings { get { return _currentHealings; } }
	public float CurrentMana { get { return _currentMana; } }
	public float Damage { get { return _currentDamage; } }
	public float ShakerForceImpact { get { return _baseStats.ShakerForceImpact; } }

	public bool ResotreHealthOnSceneChange { get { return _currentHealth <= 0; } }

	public DataSettings _dataSettings;

	private HealingObject _healingObject;
	private float _elapsedHealingDuration;

	public void Heal(HealingObject healingObject)
	{
		if (_elapsedHealingDuration <= 0)
		{
			_healingObject = healingObject;
			_elapsedHealingDuration = _healingObject.Duration;
		}
	}

	public void Init(Stats stats)
	{
		_baseStats = stats;

		_currentHealth = stats.BaseHealth;
		_currentHealings = stats.BaseHealings;
		_currentMana = stats.BaseMana;
		_elapsedHealingDuration = 0;
		_currentHealthRegeneration = stats.HealthRecovery;
		_currentLiveSuction = stats.LifeSuction;
		_currentDamage = stats.Damage;

		DataPersistenersManager.RegisterDataPersistener(this);
	}

	public void ResetHealth()
	{
		_currentHealth = _baseStats.BaseHealth;
	}

	public void ResetHealings()
	{
		_currentHealings = _baseStats.BaseHealings;
	}
	
	public void UseHealings()
	{
		_currentHealings--;
	}

	public void GetDamage(CurrentStats currentStats)
	{
		GetDamage(currentStats.Damage);
	}

	public void GetDamage(float damage)
	{
		_currentHealth -= damage;
	}
	public void HealOverTime()
	{
		_currentHealth += _currentHealthRegeneration * Time.deltaTime;
	}
	public void LiveSuctionOverDamage()
	{
		if (_currentHealth >= _baseStats.BaseHealth)
		{
			_currentHealth = _baseStats.BaseHealth;
		}
		else
		{
			_currentHealth += _baseStats.Damage * _currentLiveSuction / 100;
			Debug.Log(_baseStats.Damage * _currentLiveSuction / 100);
		}
	}
	public void LiveSuctionOverBaseHealth()
	{
		_currentHealth += _baseStats.BaseHealth * _currentLiveSuction / 100;
	}

	public void SetHealthRegen(float value)
	{
		_currentHealthRegeneration = value;
	}

	public void SetLiveSuction(float value)
	{
		_currentLiveSuction = value;
	}

	public void ResetLiveSuction()
	{
		_currentLiveSuction = _baseStats.LifeSuction;
	}

	public void ResetHealthRegeneration()
	{
		_currentHealthRegeneration = _baseStats.HealthRecovery;
	}
	public void Update()
	{
		_elapsedHealingDuration -= Time.deltaTime;

		if (_healingObject == null)
		{
			return;
		}

		if ((_currentHealth < _baseStats.BaseHealth) && (_elapsedHealingDuration > 0))
		{
			_currentHealth += _healingObject.HealthPerSecond * Time.deltaTime;
		}
		else if(_healingObject != null)
		{
			_healingObject = null;
		}
	}
	private void OnDisable()
	{
		DataPersistenersManager.UnRegisterDataPersistener(this);
	}

	public DataSettings GetDataSettings()
	{
		return _dataSettings;
	}

	public Data SaveData()
	{
		return new Data<float, float, int, bool>(_currentHealth, _currentMana, _currentHealings , ResotreHealthOnSceneChange);
	}

	public void LoadData(Data data)
	{
		Data<float, float, int, bool> statsData = (Data<float, float, int, bool>)data;
		_currentHealth = statsData.Value3 ? _baseStats.BaseHealth : statsData.Value0;
		_currentMana = statsData.Value1;
		_currentHealings = statsData.Value3 ? _baseStats.BaseHealings : statsData.Value2;
	}

	public void SetDataSettings(string dataTag, DataSettings.PersistenceType persistenceType)
	{
		_dataSettings.dataTag = dataTag;
		_dataSettings.persistenceType = persistenceType;
	}


	public void GetDamage(Stats agressorStates)
	{
		float damage = agressorStates.Damage;

		_currentHealth -= damage;
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(CurrentStats))]
public class CurrentStatsEditor : DataPersisterEditor
{ }
#endif