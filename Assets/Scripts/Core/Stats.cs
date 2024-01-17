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
	[SerializeField] private float _shakerForceImpact;
	[SerializeField] private float _liveSuction;
	
	public float BaseHealth { get { return _health; } }
	public float BaseMana { get { return _mana; } }
	public float HealthRecovery { get { return _healthRecover; } }
	public float ManaRecovery { get { return _manaRecover; } }
	public float Damage { get { return _damage; }}
	public float LifeSuction { get { return _liveSuction; } }
	public float ShakerForceImpact { get { return _shakerForceImpact; } }

	public float AttackRate { get { return _attackRate; } }	
}
