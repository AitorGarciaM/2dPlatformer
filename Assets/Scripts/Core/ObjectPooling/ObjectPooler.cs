using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
	[System.Serializable]
	public class Pool
	{
		public string tag;
		public GameObject prefab;
		public int size;
	}

	private static ObjectPooler _isntance;

	[SerializeField] private List<Pool> _pools;
	private Dictionary<string, Queue<GameObject>> _poolDictionary;

	public static ObjectPooler Instance
	{
		get
		{
			if(_isntance != null)
			{
				return _isntance;
			}

			_isntance = FindObjectOfType<ObjectPooler>();

			if(_isntance != null)
			{
				return _isntance;
			}

			Create();

			return _isntance;
		}
	}

	private static void Create()
	{
		ObjectPooler prefab = Resources.Load<ObjectPooler>("Prefabs/ObjectPooler");

		_isntance = Instantiate(prefab);
	}

    // Start is called before the first frame update
    void Start()
    {
		_poolDictionary = new Dictionary<string, Queue<GameObject>>();

		foreach(Pool pool in _pools)
		{
			Queue<GameObject> objectPool = new Queue<GameObject>();

			for (int i = 0; i < pool.size; i++)
			{
				GameObject obj = Instantiate(pool.prefab);
				obj.SetActive(false);
				objectPool.Enqueue(obj);

			}

			_poolDictionary.Add(pool.tag, objectPool);
		}
    }

	public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
	{
		if(!_poolDictionary.ContainsKey(tag))
		{
			Debug.LogWarning("Pool with tag " + tag + "doesn't exist.");
			return null;
		}

		GameObject objToSpawn = _poolDictionary[tag].Dequeue();

		objToSpawn.SetActive(true);
		objToSpawn.transform.position = position;
		objToSpawn.transform.rotation = rotation;

		_poolDictionary[tag].Enqueue(objToSpawn);

		return objToSpawn;
	}
}
