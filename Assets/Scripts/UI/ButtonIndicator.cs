using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonIndicator : MonoBehaviour
{
	[SerializeField] private List<Sprite> _buttonSprites;
	[SerializeField] private Image _image;

	private ControllerChecker.ControllerType _currentType = ControllerChecker.ControllerType.Keyboard;

    // Update is called once per frame
    void Update()
    {
        if(_currentType != ControllerChecker.Instance.CurrentControllerType)
		{
			_currentType = ControllerChecker.Instance.CurrentControllerType;

			switch (_currentType)
			{
				case ControllerChecker.ControllerType.Keyboard:
					_image.sprite = _buttonSprites[0];
					break;
				case ControllerChecker.ControllerType.Gamepad:
					_image.sprite = _buttonSprites[1];
					break;
				default:
					break;
			}
		}
    }
}
