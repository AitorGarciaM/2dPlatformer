using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Door : MonoBehaviour, IDataPersistener
{
	[SerializeField] private Vector2 _openPosition;
	[SerializeField] private Vector2 _closedPosition;

	[SerializeField] private bool _isLeft;
	[SerializeField] private bool _isOpen;
	[SerializeField] private bool _chekOpen;

	[HideInInspector]
	public DataSettings _dataSettings;

	private SpriteRenderer _spRenderer;
	private Transform _player;

	#region Data Persistance

	public DataSettings GetDataSettings()
	{
		return _dataSettings;
	}

	public void LoadData(Data data)
	{
		Data<bool> doorData = (Data<bool>)data;

		_isOpen = doorData.Value;

		CheckDoorOpen();
	}

	public Data SaveData()
	{
		return new Data<bool>(_isOpen);
	}

	public void SetDataSettings(string dataTag, DataSettings.PersistenceType persistenceType)
	{
		_dataSettings.dataTag = dataTag;
		_dataSettings.persistenceType = persistenceType;
	}

	#endregion

	public void Open()
	{
		StartCoroutine(MoveDoor(_openPosition));
	}

	public void Close()
	{
		StartCoroutine(MoveDoor(_closedPosition));
	}

	private void OnEnable()
	{
		DataPersistenersManager.RegisterDataPersistener(this);
	}

	// Start is called before the first frame update
	void Start()
    {
		_spRenderer = GetComponent<SpriteRenderer>();
		_player = FindObjectOfType<PlayerController>().transform;

		CheckDoorOpen();
    }

	private void CheckDoorOpen()
	{
		if (_isOpen)
		{
			transform.position = new Vector2(transform.position.x, _openPosition.y);
		}
		else
		{
			transform.position = new Vector2(transform.position.x, _closedPosition.y);
		}
	}

	private void Update()
	{
		if(_chekOpen)
		{
			if(_isOpen)
			{
				Close();

				_chekOpen = false;
			}
			else
			{
				Open();

				_chekOpen = false;
			}
		}
	}

	private void LateUpdate()
	{
		if((_player.position.x < transform.position.x && _isLeft) || (_player.position.x > transform.position.x && !_isLeft))
		{
			transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
			Debug.Log("A");
		}
		else if((_player.position.x > transform.position.x && _isLeft) || (_player.position.x < transform.position.x && !_isLeft))
		{
			transform.position = new Vector3(transform.position.x, transform.position.y, -0.5f);
			Debug.Log("B");
		}
	}

	private void OnDestroy()
	{
		DataPersistenersManager.UnRegisterDataPersistener(this);
	}

	private IEnumerator MoveDoor(Vector2 newPosition)
	{
		bool endMovement = false;

		while (!endMovement)
		{
			if (Mathf.Abs(transform.position.y - newPosition.y) < 0.01f)
			{
				endMovement = true;
				_isOpen = !_isOpen;
				break;
			}

			transform.position = new Vector2(transform.position.x, Mathf.MoveTowards(transform.position.y, newPosition.y, Time.deltaTime));
			yield return null;
		}

		yield break;
	}
   
}

#region Editor

#if UNITY_EDITOR
[CustomEditor(typeof(Door))]
public class DoorEditor : DataPersisterEditor
{ }
#endif

#endregion