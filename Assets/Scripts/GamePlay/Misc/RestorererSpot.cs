using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(HealthRestorerer), typeof(PotionRestorerer), typeof(CheckPoint))]
public class RestorererSpot : MonoBehaviour, Iinteractable
{
	[SerializeField] private CanvasGroup _canvasGroup;

	private HealthRestorerer _healthRestorer;
	private PotionRestorerer _potionRestorer;
	private CheckPoint _checkPoint;

	private void Awake()
	{
		_healthRestorer = GetComponent<HealthRestorerer>();
		_potionRestorer = GetComponent<PotionRestorerer>();
		_checkPoint = GetComponent<CheckPoint>();
	}

	public void Interact()
	{
		_healthRestorer.RestoreHealth();
		_potionRestorer.PotionRestore();
		_checkPoint.SetCheckPoint();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if(collision.tag == "Player")
		{
			collision.GetComponent<PlayerController>().SetInteractable(this);
			_canvasGroup.alpha = 1;
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if(collision.tag == "Player")
		{
			collision.GetComponent<PlayerController>().SetInteractable(null);
			_canvasGroup.alpha = 0;
		}
	}
}
