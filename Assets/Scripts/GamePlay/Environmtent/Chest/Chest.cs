using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class Chest : MonoBehaviour, Iinteractable, IDataPersistener
{
	[SerializeField] private ChestSO _chestData;

	[SerializeField] private AnimationHnadler _animationHandler;
	[SerializeField] private CanvasGroup _interactGroup;
	[SerializeField] private CanvasGroup _treasureCanvas;
	[SerializeField] private TextMeshProUGUI _text;
	[SerializeField] private Image _image;

	[SerializeField] private float _treasurePopupTime;
	[SerializeField] private float _treasureFadeDuration;
	
	private TrinketsManager _trinketsManager;
	private bool _alreadyOpen;
	private bool _isOpen;

	[HideInInspector]
	public DataSettings _dataSettings;
	
	public Treasure GetTreasure() { return _chestData.Treasure; }

	public void Interact()
	{
		StartCoroutine(Open());
	}

	private void OnEnable()
	{
		_trinketsManager = FindObjectOfType<TrinketsManager>();
		DataPersistenersManager.RegisterDataPersistener(this);
	}

	private void OnDisable()
	{
		DataPersistenersManager.UnRegisterDataPersistener(this);
	}

	private void Start()
	{
	}

	private void Update()
	{
		if(_isOpen && !_alreadyOpen)
		{
			_animationHandler.SetBool("Open", _isOpen);
			_alreadyOpen = true;
		}
	}

	private IEnumerator Open()
	{
		if(_isOpen)
		{
			StopCoroutine(Open());
		}

		_interactGroup.alpha = 0;

		_animationHandler.SetBool("Opening", true);

		// Waits until chest is open.
		while (_animationHandler.AnimationISFinished("Opening"))
		{
			yield return null;
		}

		_isOpen = true;

		// The treasure pop up appears on the screen.
		_treasureCanvas.gameObject.SetActive(true);

		_text.text = _chestData.Treasure.Name;
		_image.sprite = _chestData.Treasure.GetSprite();

		_treasureCanvas.alpha = 1;

		// If treasure is a trinket stores it.
		Trinket trinket = (Trinket)_chestData.Treasure;

		if(trinket != null)
		{
			_trinketsManager.AddTrinket(trinket);
		}

		yield return new WaitForSeconds(_treasurePopupTime);

		// Fades out the treasure pop up.
		float speed = Mathf.Abs(_treasureCanvas.alpha - 0) / _treasureFadeDuration;
		
		while(!Mathf.Approximately(_treasureCanvas.alpha, 0))
		{
			_treasureCanvas.alpha = Mathf.MoveTowards(_treasureCanvas.alpha, 0, speed * Time.deltaTime);
			yield return null;
		}

		// Deactivates the treasure pop up.
		_treasureCanvas.alpha = 0;
		_treasureCanvas.gameObject.SetActive(false);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "Player" && !_isOpen)
		{
			collision.GetComponent<PlayerController>().SetInteractable(this);
			_interactGroup.alpha = 1;
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.tag == "Player")
		{
			collision.GetComponent<PlayerController>().SetInteractable(null);
			_interactGroup.alpha = 0;
		}
	}

	public DataSettings GetDataSettings()
	{
		return _dataSettings;
	}

	public Data SaveData()
	{
		return new Data<bool>(_isOpen);
	}

	public void LoadData(Data data)
	{
		Data<bool> chestData = (Data<bool>)data;
		_isOpen = chestData.Value;
	}

	public void SetDataSettings(string dataTag, DataSettings.PersistenceType persistenceType)
	{
		_dataSettings.dataTag = dataTag;
		_dataSettings.persistenceType = persistenceType;
	}
}

[CustomEditor(typeof(Chest)), CanEditMultipleObjects]
public class ChestEditor : DataPersisterEditor
{}
