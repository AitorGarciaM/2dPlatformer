using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VideoSetter : MonoBehaviour
{
	[SerializeField] Toggle _vSyncToggle;
	[SerializeField] Toggle _fpsToggle;
	[SerializeField] TMP_Dropdown _resolutionDropDown;

	private Resolution[] _resolutions;
	private List<Resolution> _filteredResolutions;

	private int _currentRefreshRate;
	private int _currentResolutionIndex;

	private void Awake()
	{
		_vSyncToggle.onValueChanged.AddListener(SetVSync);
		_fpsToggle.onValueChanged.AddListener(SetFrameRateCap);

		_fpsToggle.isOn = false;

		_resolutions = Screen.resolutions;
		_filteredResolutions = new List<Resolution>();

		_resolutionDropDown.ClearOptions();

		_currentRefreshRate = Screen.currentResolution.refreshRate;

		for (int i = 0; i < _resolutions.Length; i++)
		{
			if(_resolutions[i].refreshRate == _currentRefreshRate)
			{
				_filteredResolutions.Add(_resolutions[i]);
			}
		}

		List<string> options = new List<string>();

		for (int i = 0; i < _filteredResolutions.Count; i++)
		{
			options.Add(_filteredResolutions[i].width.ToString() + "x" + _filteredResolutions[i].height.ToString() + " " + _filteredResolutions[i].refreshRate.ToString() + " HZ");

			if(_filteredResolutions[i].width == Screen.width && _filteredResolutions[i].height == Screen.height)
			{
				_currentResolutionIndex = i;
			}
		}

		_resolutionDropDown.AddOptions(options);
		_resolutionDropDown.value = _currentResolutionIndex;
		_resolutionDropDown.RefreshShownValue();
		_resolutionDropDown.onValueChanged.AddListener(SetResolution);
	}

	public void SetVSync(bool value)
	{
		if (value)
		{
			QualitySettings.vSyncCount = 1;
		}
		else
		{
			QualitySettings.vSyncCount = 0;
		}
	}

	public void SetFrameRateCap(bool value)
	{
		if(value)
		{
			Application.targetFrameRate = 30;
		}
		else
		{
			Application.targetFrameRate = -1;
		}
	}

	public void SetResolution(int index)
	{
		Resolution selectedResolution = _filteredResolutions[index];

		Screen.SetResolution(selectedResolution.width, selectedResolution.height, true);
	}
}
