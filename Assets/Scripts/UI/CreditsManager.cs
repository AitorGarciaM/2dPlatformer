using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CreditsManager : MonoBehaviour
{
	[SerializeField] private Image _image;

	PlayerAction _input;

	private void Awake()
	{
		_input = new PlayerAction();
		_input.Enable();
		_input.Credits.Skip.started += OnSkipStarted;
		_input.Credits.Skip.canceled += OnSkipCanceled;
	}

	private void OnDisable()
	{
		_input.Disable();
	}

	private void OnSkipStarted(InputAction.CallbackContext context)
	{
		_image.gameObject.SetActive(true);
		StartCoroutine(RadialBar());
	}

	private void OnSkipInProgress(InputAction.CallbackContext context)
	{

	}

	private void OnSkipCanceled(InputAction.CallbackContext context)
	{
		StopCoroutine(RadialBar());
		_image.fillAmount = 0;
		_image.gameObject.SetActive(false);
	}

	private void UpdateProgressBar()
	{
		_image.fillAmount += Mathf.MoveTowards(_image.fillAmount, 1, Time.deltaTime);
	}

	public void ReturnToMenu()
	{
		StopCoroutine(RadialBar());
		SceneController.Instance.TransitionToMenu();
	}

	IEnumerator RadialBar()
	{
		if(_image == null)
		{
			yield break;
		}

		float speed = Mathf.Abs(0 - 1);

		while(!Mathf.Approximately(_image.fillAmount, 1))
		{
			_image.fillAmount = Mathf.MoveTowards(_image.fillAmount, _image.fillAmount + speed, Time.deltaTime);
			yield return null;
		}

		_image.fillAmount = 1;
		ReturnToMenu();

		yield break;
	}

	private void OnDestroy()
	{
		
	}

}
