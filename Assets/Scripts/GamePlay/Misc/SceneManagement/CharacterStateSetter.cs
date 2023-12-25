using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;

#if UNITY_EDITOR

#region CharacterStateSetterEditor

[CustomEditor(typeof(CharacterStateSetter))]
class CharacterStateSetterEditor : Editor
{
	SerializedProperty _playerControllerProp;
	SerializedProperty _setCharacterVelocityProp;
	SerializedProperty _characterVelocityProp;
	SerializedProperty _setCharacterFacingLeftProp;
	SerializedProperty _facingLeftProp;
	SerializedProperty _setStateProp;
	SerializedProperty _animatorPropertySetterProp;
	SerializedProperty _animationHanderlProp;

	private void OnEnable()
	{
		_playerControllerProp = serializedObject.FindProperty("_player");
		_setCharacterVelocityProp = serializedObject.FindProperty("_setCharacterVelocity");
		_characterVelocityProp = serializedObject.FindProperty("_characterVelocity");
		_setCharacterFacingLeftProp = serializedObject.FindProperty("_setCharacterFacing");
		_facingLeftProp = serializedObject.FindProperty("_faceLeft");
		_setStateProp = serializedObject.FindProperty("_setState");
		_animatorPropertySetterProp = serializedObject.FindProperty("_animatorParameterSetter");
		_animationHanderlProp = serializedObject.FindProperty("_animationHandler");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		// Parameters to show.
		EditorGUILayout.PropertyField(_playerControllerProp);
		EditorGUILayout.PropertyField(_setCharacterVelocityProp);

		if (_setCharacterVelocityProp.boolValue == true)
		{
			EditorGUILayout.PropertyField(_characterVelocityProp);
		}

		EditorGUILayout.PropertyField(_setCharacterFacingLeftProp);

		if (_setCharacterFacingLeftProp.boolValue == true)
		{
			EditorGUILayout.PropertyField(_facingLeftProp);
		}

		EditorGUILayout.PropertyField(_setStateProp);

		if (_setStateProp.boolValue == true)
		{
			EditorGUILayout.PropertyField(_animatorPropertySetterProp);
			EditorGUILayout.PropertyField(_animationHanderlProp);
		}

		serializedObject.ApplyModifiedProperties();
	}
}
#endregion

#endif

public class CharacterStateSetter : MonoBehaviour
{
	[System.Serializable]
	public struct ParameterSetter
	{
		public enum ParameterType
		{
			Bool, Float, Int, Trigger
		}

		public ParameterType parameterType;
		public string parameterName;
		public bool boolValue;
		public float floatValue;
		public int intValue;
	}

	

	[SerializeField] private PlayerController _player;
	[SerializeField] private bool _setCharacterVelocity;
	[SerializeField] private Vector2 _characterVelocity;
	[SerializeField] private bool _setCharacterFacing;
	[SerializeField] private bool _faceLeft;
	[SerializeField] private bool _setState;
	[SerializeField] private ParameterSetter _animatorParameterSetter;
	[SerializeField] private AnimationHnadler _animationHandler;

	
	public void SetCharacterState()
	{
		Debug.Log(gameObject.name + " " + gameObject.scene.name);

		if(_setCharacterVelocity)
		{
			 _player.SetForce(_characterVelocity);
		}

		if (_setCharacterFacing)
		{
			_player.UpdateFacing(_faceLeft);
		}

		if(_setState)
		{
			switch (_animatorParameterSetter.parameterType)
			{
				case ParameterSetter.ParameterType.Bool:
					_animationHandler.SetBool(_animatorParameterSetter.parameterName, _animatorParameterSetter.boolValue);
					break;
				case ParameterSetter.ParameterType.Float:
					_animationHandler.SetFloat(_animatorParameterSetter.parameterName, _animatorParameterSetter.floatValue);
					break;
				case ParameterSetter.ParameterType.Int:
					_animationHandler.SetInteger(_animatorParameterSetter.parameterName, _animatorParameterSetter.intValue);
					break;
				case ParameterSetter.ParameterType.Trigger:
					_animationHandler.SetTrigger(_animatorParameterSetter.parameterName);
					break;
			}
		}
	}
}
