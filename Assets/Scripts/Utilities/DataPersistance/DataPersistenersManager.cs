using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataPersistenersManager : MonoBehaviour
{
	private static DataPersistenersManager _instance;

	private static bool s_quitting;
	public static DataPersistenersManager Instance
	{
		get
		{
			if (_instance != null)
			{
				return _instance;
			}

			_instance = FindObjectOfType<DataPersistenersManager>();

			if (_instance != null)
			{
				return _instance;
			}

			Create();

			return _instance;
		}
	}

	private static void Create()
	{
		GameObject instance = Instantiate(Resources.Load<GameObject>("Prefabs/DataPersistenersManager"));
		DontDestroyOnLoad(instance);
		_instance = instance.GetComponent<DataPersistenersManager>();
	}

	private HashSet<IDataPersistener> _dataPersisteners = new HashSet<IDataPersistener>();
	private Dictionary<string, Data> _storedData = new Dictionary<string, Data>();

	private System.Action _functionHandler;


	private void Awake()
	{
		if (Instance != this)
		{
			Destroy(gameObject);
		}
	}

	private void Update()
	{
		if (_functionHandler != null)
		{
			_functionHandler.Invoke();
			_functionHandler = null;
		}
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			s_quitting = true;
		}
	}

	private void Register(IDataPersistener dataPersistener)
	{
		_functionHandler += () =>
		{
			_dataPersisteners.Add(dataPersistener);
		};
	}

	private void UnRegister(IDataPersistener dataPersistener)
	{
		_functionHandler += () =>
		{
			_dataPersisteners.Remove(dataPersistener);
		};
	}

	private void Save(IDataPersistener dataPersistener)
	{
		var dataSettings = dataPersistener.GetDataSettings();

		if (dataSettings.persistenceType == DataSettings.PersistenceType.ReadOnly)
		{
			return;
		}

		if (!string.IsNullOrEmpty(dataSettings.dataTag))
		{
			_storedData[dataSettings.dataTag] = dataPersistener.SaveData();
		}
	}

	private void SaveAllDataInternal()
	{
		foreach (var dp in _dataPersisteners)
		{
			Save(dp);
		}
	}

	private void LoadAllDataInternal()
	{
		_functionHandler += () =>
		{
			foreach (var dp in _dataPersisteners)
			{
				var dataSetting = dp.GetDataSettings();

				if (!string.IsNullOrEmpty(dataSetting.dataTag))
				{
					if (_storedData.ContainsKey(dataSetting.dataTag))
					{
						dp.LoadData(_storedData[dataSetting.dataTag]);
					}
				}
			}
		};
	}

	public static void RegisterDataPersistener(IDataPersistener dataPersistener)
	{
		Instance.Register(dataPersistener);
	}

	public static void UnRegisterDataPersistener(IDataPersistener dataPersistener)
	{ 
		if(!s_quitting)
		{
			Instance.UnRegister(dataPersistener);
		}
	}

	public static void SaveAllData()
	{
		Instance.SaveAllDataInternal();
	}

	public static void LoadAllData()
	{
		Instance.LoadAllDataInternal();
	}

	public static void ClearAllDataPersisteners()
	{
		Instance._dataPersisteners.Clear();
	}
}
