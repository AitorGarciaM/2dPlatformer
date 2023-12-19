using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Health Regeneration Effect", fileName = "HealthRegenerationEffect")]
public class HealthRegenerationEffect : Effect
{
	[SerializeField] private float _value;

	public override void Apply(GameObject gameObject)
	{
		gameObject.GetComponent<Stats>().SetHealthRegen(_value);
	}

	public override void Disapply(GameObject gameObject)
	{
		gameObject.GetComponent<IHitable>().GetStats().ResetHealthRegeneration();
	}
}
