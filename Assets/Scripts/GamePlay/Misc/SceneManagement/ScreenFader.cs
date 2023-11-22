using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
	private static ScreenFader s_instance;

	[SerializeField] private float _fadeDuration = 1f;
	[SerializeField] private CanvasGroup _faderCanvasGroup;
	private bool _isFading;


	public static ScreenFader Instance
	{
		get
		{
			if(s_instance != null)
			{
				return s_instance;
			}

			s_instance = FindObjectOfType<ScreenFader>();

			if(s_instance != null)
			{
				return s_instance;
			}

			Create();

			return s_instance;
		}
	}
	public bool IsFading { get { return _isFading; } }
	
	private static void Create()
	{
		ScreenFader controllerPrefab = Resources.Load<ScreenFader>("Prefabs/SceneFader");
		s_instance = Instantiate(controllerPrefab);
	}

	private void Awake()
	{
		if(s_instance != null)
		{
			Destroy(gameObject);
			return;
		}

		DontDestroyOnLoad(gameObject);
	}

	private IEnumerator Fade(float finalAlpha, CanvasGroup canvasGroup)
	{
		_isFading = true;
		canvasGroup.blocksRaycasts = true;
		float fadeSpeed = Mathf.Abs(canvasGroup.alpha - finalAlpha) / _fadeDuration;
		while(!Mathf.Approximately(canvasGroup.alpha, finalAlpha))
		{
			canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, finalAlpha, fadeSpeed * Time.deltaTime);
			yield return null;
		}

		canvasGroup.alpha = finalAlpha;
		_isFading = false;
		canvasGroup.blocksRaycasts = false;
		yield return null;
	}

	public IEnumerator FadeSceneOut()
	{
		_faderCanvasGroup.gameObject.SetActive(true);

		yield return Instance.StartCoroutine(Instance.Fade(1f, _faderCanvasGroup));
	}

	public IEnumerator FadeSceneIn()
	{
		yield return Instance.StartCoroutine(Instance.Fade(0f, _faderCanvasGroup));

		_faderCanvasGroup.gameObject.SetActive(false);
	}
}
