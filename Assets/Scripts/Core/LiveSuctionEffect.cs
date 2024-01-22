using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Effects/Live Suction Effect", fileName ="LiveSuctionEffect")]
public class LiveSuctionEffect : Effect
{
	[SerializeField] private float _value;

	public override void Apply(GameObject gameObject)
	{
		gameObject.GetComponent<IHitable>().GetStats().SetLiveSuction(_value);
	}

	public override void Disapply(GameObject gameObject)
	{
		gameObject.GetComponent<IHitable>().GetStats().ResetLiveSuction();
	}

	public override string GetValue()
	{
		return _value.ToString();
	}
}
