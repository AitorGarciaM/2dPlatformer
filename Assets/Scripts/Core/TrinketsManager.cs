using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;

public class TrinketsManager : MonoBehaviour, IDataPersistener
{
	[HideInInspector]
	public DataSettings _dataSettings;
	[SerializeField]
	private GameObject _trinketsMenu;

	private List<Trinket> _trinkets;
	private Dictionary<string, TrinketSlot> _trinketsSlotsDict = new Dictionary<string, TrinketSlot>();
	private EquipedSlot[] _equipedSlots;

	private TrinketSlot _selectedTrinketSlot;

	public void AddTrinket(Trinket newTrinket)
	{
		_trinkets.Add(newTrinket);

		_trinketsSlotsDict[newTrinket.name].ActiveSlot(newTrinket);
	}

	public void SelectTrinketSlot(TrinketSlot trinketSlot)
	{

		if(trinketSlot.IsEquiped)
		{
			for (int i = 0; i < _equipedSlots.Length; i++)
			{
				if(trinketSlot.GetTrinket().EquipedSlotId == _equipedSlots[i].Id)
				{
					_equipedSlots[i].Unequip();
					return;
				}
			}
		}

		_selectedTrinketSlot = trinketSlot;
		EventSystem.current.SetSelectedGameObject(_equipedSlots[0].gameObject);
	}

	public void SetEquipedSlot(EquipedSlot equipedSlot)
	{
		equipedSlot.Equip(_selectedTrinketSlot);
	}

	public void Unequip(EquipedSlot equipedSlot)
	{
		equipedSlot.Unequip();
	}

	public DataSettings GetDataSettings()
	{
		return _dataSettings;
	}

	public void LoadData(Data data)
	{
		Data<List<Trinket>> trinketsData = (Data<List<Trinket>>)data;

		_trinkets = trinketsData.Value;

		Debug.Log("Number of Trinkets: " + _trinkets.Count);

		// Activates all the Trinkets slots that player found.
		foreach (Trinket trinket in _trinkets)
		{
			bool slotActive = _trinketsSlotsDict[trinket.name].ActiveSlot(trinket);
			
			if (trinket.EquipedSlotId >= 0 && trinket != null && slotActive)
			{
				for (int j = 0; j < _equipedSlots.Length; j++)
				{
					if (trinket.EquipedSlotId == _equipedSlots[j].Id)
					{
						_equipedSlots[j].Equip(_trinketsSlotsDict[trinket.name]);
					}
				}
			}
		}
	}
	

	public Data SaveData()
	{
		return new Data<List<Trinket>>(_trinkets);
	}

	public void SetDataSettings(string dataTag, DataSettings.PersistenceType persistenceType)
	{
		_dataSettings.dataTag = dataTag;
		_dataSettings.persistenceType = persistenceType;
	}

	private void OnEnable()
	{
		DataPersistenersManager.RegisterDataPersistener(this);
		_trinkets = new List<Trinket>();
	}

	private void OnDisable()
	{
		DataPersistenersManager.UnRegisterDataPersistener(this);
	}

	private void Awake()
	{
		TrinketSlot[] trinketsSlots = FindObjectsOfType<TrinketSlot>();

		for (int i = 0; i < trinketsSlots.Length; i++)
		{
			_trinketsSlotsDict.Add(trinketsSlots[i].GetTrinket().name, trinketsSlots[i]);
		}

		_equipedSlots = FindObjectsOfType<EquipedSlot>();
		_trinketsMenu.SetActive(false);
	}

	private void OnApplicationQuit()
	{
		foreach(Trinket trinket in _trinkets)
		{
			trinket.Reset();
		}
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(TrinketsManager))]
public class TrinketsManagerEditor : DataPersisterEditor
{}
#endif
