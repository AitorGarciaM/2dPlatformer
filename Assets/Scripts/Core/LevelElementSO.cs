using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ScriptableObjectIdAttribute : PropertyAttribute { }

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(ScriptableObjectIdAttribute))]
public class ScriptableObjectIdDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		GUI.enabled = false;
		if(string.IsNullOrEmpty(property.stringValue))
		{
			property.stringValue = System.Guid.NewGuid().ToString();
		}

		EditorGUI.PropertyField(position, property, label, true);
		GUI.enabled = true;
	}
}

#endif

public abstract class LevelElementSO : ScriptableObject
{
	[ScriptableObjectId, SerializeField]
	private string _id;

	public string Id { get { return _id; } }
}
