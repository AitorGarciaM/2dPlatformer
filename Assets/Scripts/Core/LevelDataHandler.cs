using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LevelDataHandler : MonoBehaviour, IDataPersistener
{
	private Dictionary<string, LevelElementSO> _elementsDictionary;

	[HideInInspector]
	public DataSettings _dataSettings;

	private void OnEnable()
	{
		DataPersistenersManager.RegisterDataPersistener(this);
	}

	private void OnDisable()
	{
		DataPersistenersManager.UnRegisterDataPersistener(this);
	}

	private void Start()
	{
		LevelElementSO[] levelElements = FindObjectsOfType<LevelElementSO>();

		foreach(var levelElement in levelElements)
		{
			if(_elementsDictionary.ContainsKey(levelElement.Id))
			{
			}
			else
			{
				_elementsDictionary[levelElement.Id] = levelElement;
			}
		}
	}

	public void AddElement(LevelElementSO elementData)
	{
		_elementsDictionary[elementData.Id] = elementData;
	}

	public DataSettings GetDataSettings()
	{
		return _dataSettings;
	}

	public LevelElementSO GetElement(string id)
	{
		if(_elementsDictionary.ContainsKey(id))
		{
			return _elementsDictionary[id];
		}

		Debug.LogWarning("Element with id: " + id + "has not been found.");

		return null;
	}

	public void LoadData(Data data)
	{
		Data<Dictionary<string, LevelElementSO>> levelData = (Data<Dictionary<string, LevelElementSO>>)data;

		_elementsDictionary = levelData.Value;
	}

	public Data SaveData()
	{
		return new Data<Dictionary<string, LevelElementSO>>(_elementsDictionary);
	}

	public void SetDataSettings(string dataTag, DataSettings.PersistenceType persistenceType)
	{
		_dataSettings.dataTag = dataTag;
		_dataSettings.persistenceType = persistenceType;
	}
}

[CustomEditor(typeof(LevelDataHandler))]
public class LevelDataHandlerEditor : DataPersisterEditor
{}
