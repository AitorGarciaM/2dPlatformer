using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
[RequireComponent(typeof(HealthRestorerer), typeof(PotionRestorerer), typeof(CheckPoint))]
public class RestorererSpot : MonoBehaviour, Iinteractable
{
	[SerializeField] private CanvasGroup _canvasGroup;
	[SerializeField] private TextMeshProUGUI _restedText;
	[SerializeField] private RandomAudioPlayer _restorerPlayer;
	[SerializeField] private float _textFadeSpeed;

	private PlayerController _player;
	private HealthRestorerer _healthRestorer;
	private PotionRestorerer _potionRestorer;
	private CheckPoint _checkPoint;

	private void Awake()
	{
		_healthRestorer = GetComponent<HealthRestorerer>();
		_potionRestorer = GetComponent<PotionRestorerer>();
		_checkPoint = GetComponent<CheckPoint>();

		_player = FindObjectOfType<PlayerController>();
	}

	public void Interact()
	{
		_healthRestorer.RestoreHealth();
		_potionRestorer.PotionRestore();
		_checkPoint.SetCheckPoint();

		StartCoroutine(FadeInText());
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
	
	private IEnumerator FadeOutText()
	{
		Color textColor = _restedText.color;

		while(_restedText.color.a > 0)
		{
			textColor.a -= _textFadeSpeed * Time.deltaTime;
			_restedText.color = textColor;
			yield return new WaitForEndOfFrame();
		}

		textColor.a = 0;
		_restedText.color = textColor;

		_player.RestartControl();

		yield break;
	}

	private IEnumerator FadeInText()
	{
		Color textColor = _restedText.color;

		_player.PauseControl();

		while (_restedText.color.a > 0)
		{
			textColor.a += _textFadeSpeed * Time.deltaTime;
			_restedText.color = textColor;
			yield return new WaitForEndOfFrame();
		}

		textColor.a = 1;
		_restedText.color = textColor;
		_restorerPlayer.PlayRandomSound();

		yield return StartCoroutine(FadeOutText());

		yield break;
	}
}
