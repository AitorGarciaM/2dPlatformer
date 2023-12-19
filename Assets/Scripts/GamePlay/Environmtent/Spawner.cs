using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Spawner : MonoBehaviour
{
	public enum SpawnMethod
	{
		Random,
		RoundRobin
	}

	[SerializeField] private string _objectTag;
	[SerializeField] private int _quantity;
	[SerializeField] SpawnMethod _spawnMethod;
	[SerializeField] private int _roundsActive;

	private ObjectPooler _objectPooler;
	private int _currentRound;

	public List<GameObject> SpawnObjects()
	{
		List<GameObject> spawnedObjects = new List<GameObject>();

		for (int i = 0; i < _quantity; i++)
		{
			spawnedObjects.Add(_objectPooler.SpawnFromPool(_objectTag, transform.position, Quaternion.identity));
		}

		return spawnedObjects;
	}

    // Start is called before the first frame update
    void Start()
    {
		_objectPooler = ObjectPooler.Instance;
    }
}

#region SpawnerEditor
[CustomEditor(typeof(Spawner))]
public class SpawnerEditor : Editor
{
	private SerializedProperty _objectTagProp;
	private SerializedProperty _quantityProp;
	private SerializedProperty _spawnMethodProp;
	private SerializedProperty _roundsActiveMethod;

	private void OnEnable()
	{
		
	}

	public override void OnInspectorGUI()
	{
		
	}
}
#endregion