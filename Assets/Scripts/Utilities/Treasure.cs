using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class Treasure : ScriptableObject
{
	[SerializeField] protected string _name;
	[SerializeField] protected string _description;
	[SerializeField] protected Sprite _sprite;

	public string Name { get { return _name; } }
	public string Description { get { return _description; } }
	public Sprite GetSprite() { return _sprite; }
}
