using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public interface IDataPersistener
{
	DataSettings GetDataSettings();
	Data SaveData();
	void LoadData(Data data);

	void SetDataSettings(string dataTag, DataSettings.PersistenceType persistenceType);
}

#if UNITY_EDITOR
public abstract class DataPersisterEditor : Editor
{
	IDataPersistener _dataPersister;

	protected virtual void OnEnable()
	{
		_dataPersister = (IDataPersistener)target;
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		DataPersisterGUI(_dataPersister);
	}

	public static void DataPersisterGUI(IDataPersistener dataPersistener)
	{
		DataSettings dataSettings = dataPersistener.GetDataSettings();

		DataSettings.PersistenceType persistenceType = (DataSettings.PersistenceType)EditorGUILayout.EnumPopup("Persistence Type", dataSettings.persistenceType);
		string dataTag = EditorGUILayout.TextField("Data Tag", dataSettings.dataTag);

		dataPersistener.SetDataSettings(dataTag, persistenceType);
	}
}
#endif