using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class BackgroundMusicPlayer : MonoBehaviour
{
	#region Singletone
	private static BackgroundMusicPlayer _instance;

	public static BackgroundMusicPlayer Instance
	{
		get
		{
			if(_instance != null)
			{
				return _instance;
			}

			_instance = FindObjectOfType<BackgroundMusicPlayer>();

			if(_instance != null)
			{
				return _instance;
			}

			Create();

			return _instance;
		}
	}

	private static void Create()
	{
		BackgroundMusicPlayer pref = Resources.Load<BackgroundMusicPlayer>("Prefabs/Audio/BackgroundMusicPlayer");

		_instance = Instantiate(pref);
	}
	#endregion

	[Header("Music Settings")]
	[SerializeField] private AudioClip _musicClip;
	[SerializeField] private AudioMixerGroup _musicOutput;
	[SerializeField] private bool _playMusicOnAwake;
	[SerializeField, Range(0, 1)] private float _musicVolume;

	[Header("Ambient Settings")]
	[SerializeField] private AudioClip _ambientClip;
	[SerializeField] private AudioMixerGroup _ambientOutput;
	[SerializeField] private bool _playAmbientOnAwake;
	[SerializeField, Range(0, 1)] private float _ambientVolume;

	private BackgroundMusicPlayer _oldInstanceToDestoy = null;

	private AudioSource _musicAudioSource;
	private AudioSource _ambientAudioSource;

	private bool _transferMusicTime, _transferAmbientTime;

	private Stack<AudioClip> _musicStack = new Stack<AudioClip>();

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			if (Instance._musicClip == _musicClip)
			{
				_transferMusicTime = true;
			}

			if(Instance._ambientClip == _ambientClip)
			{
				_transferAmbientTime = true;
			}

			_oldInstanceToDestoy = Instance;
		}


		DontDestroyOnLoad(gameObject);

		_musicAudioSource = gameObject.AddComponent<AudioSource>();
		_musicAudioSource.clip = _musicClip;
		_musicAudioSource.outputAudioMixerGroup = _musicOutput;
		_musicAudioSource.loop = true;
		_musicAudioSource.volume = _musicVolume;

		if(_playMusicOnAwake)
		{
			_musicAudioSource.time = 0;
			_musicAudioSource.Play();
		}

		_ambientAudioSource = gameObject.AddComponent<AudioSource>();
		_ambientAudioSource.clip = _ambientClip;
		_ambientAudioSource.outputAudioMixerGroup = _ambientOutput;
		_ambientAudioSource.loop = true;
		_musicAudioSource.volume = _musicVolume;

		if(_playAmbientOnAwake)
		{
			_ambientAudioSource.time = 0;
			_ambientAudioSource.Play();
		}
	}

	private void Start()
	{
		if(_oldInstanceToDestoy != null)
		{
			if(_transferAmbientTime)
			{
				//_ambientAudioSource.timeSamples = _oldInstanceToDestoy._ambientAudioSource.timeSamples;
				_ambientAudioSource.time = _oldInstanceToDestoy._ambientAudioSource.time;
			}

			if(_transferMusicTime)
			{
				//_musicAudioSource.timeSamples = _oldInstanceToDestoy._musicAudioSource.timeSamples;
				_musicAudioSource.time = _oldInstanceToDestoy._musicAudioSource.time;
			}

			_oldInstanceToDestoy.Stop();
			Destroy(_oldInstanceToDestoy.gameObject);
		}
	}

	private void Update()
	{
		if(_musicStack.Count > 0)
		{
			if(!_musicAudioSource.isPlaying)
			{
				_musicStack.Pop();
				if(_musicStack.Count > 0)
				{
					_musicAudioSource.clip = _musicStack.Peek();
					_musicAudioSource.Play();
				}
			}
		}
	}

	public void PushClip(AudioClip clip, bool loop = true)
	{
		_musicStack.Push(clip);
		_musicAudioSource.Stop();
		_musicAudioSource.time = 0;
		_musicAudioSource.clip = clip;
		_musicAudioSource.loop = loop;
		_musicAudioSource.Play();
	}

	public void ChangeMusic(AudioClip clip)
	{
		_musicClip = clip;
		_musicAudioSource.clip = clip;
	}

	public void ChangeAmbient(AudioClip clip)
	{
		_ambientClip = clip;
		_ambientAudioSource.clip = clip;
	}

	public void Play()
	{
		PlayJustAmbient();
		PlayJustMusic();
	}

	public void PlayJustAmbient()
	{
		_ambientAudioSource.Play();
	}

	public void PlayJustMusic()
	{
		_musicAudioSource.Play();
	}

	public void Stop()
	{
		StopJustAmbient();
		StopJustMusic();
	}

	public void StopJustAmbient()
	{
		_ambientAudioSource.Stop();
	}

	public void StopJustMusic()
	{
		_musicAudioSource.Stop();
	}

	public void Mute()
	{
		MuteJustAmbient();
		MuteJustMusic();
	}

	public void MuteJustAmbient()
	{
		_ambientAudioSource.volume = 0;
	}

	public void MuteJustMusic()
	{
		_musicAudioSource.volume = 0;
	}

	public void UnMute()
	{
		UnMuteJustAmbient();
		UnMuteJustMusic();
	}

	public void UnMuteJustAmbient()
	{
		_ambientAudioSource.volume = _ambientVolume;
	}

	public void UnMuteJustMusic()
	{
		_musicAudioSource.volume = _musicVolume;
	}

	public void Mute (float fadeTime)
	{
		MuteJustAmbient(fadeTime);
		MuteJustMusic(fadeTime);
	}

	public void MuteJustAmbient(float fadeTime)
	{
		StartCoroutine(VolumeFade(_ambientAudioSource, 0, fadeTime));
	}

	public void MuteJustMusic(float fadeTime)
	{
		StartCoroutine(VolumeFade(_musicAudioSource, 0, fadeTime));
	}

	public void UnMute(float fadeTime)
	{
		UnMuteJustAmbient(fadeTime);
		UnMuteJustMusic(fadeTime);
	}

	public void UnMuteJustAmbient(float fadeTime)
	{
		StartCoroutine(VolumeFade(_ambientAudioSource, 1, fadeTime));
	}

	public void UnMuteJustMusic(float fadeTime)
	{
		StartCoroutine(VolumeFade(_musicAudioSource, 1, fadeTime));
	}

	private IEnumerator VolumeFade(AudioSource audioSource, float finalVolume, float fadeTime)
	{
		float volumeDifference = Mathf.Abs(audioSource.volume - finalVolume);
		float inverseFadeTime = 1 / fadeTime;

		while(!Mathf.Approximately(audioSource.volume, finalVolume))
		{
			float delta = Time.deltaTime * volumeDifference * inverseFadeTime;
			audioSource.volume = Mathf.MoveTowards(audioSource.volume, finalVolume, delta);
			yield return null;
		}

		audioSource.volume = finalVolume;

		yield break;
	}
}
