using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class Stats : ScriptableObject
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

	private float _currentHealth;
	private float _currentMana;

	public void Init()
	{
		_currentHealth = BaseHealth;
		_currentMana = BaseMana;
	}

	public void GetDamage(Stats agressorStates)
	{
		float damage = agressorStates.Damage;

		_currentHealth -= damage;
	}
}
