using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsManager : MonoBehaviour
{
	public void ReturnToMenu()
	{
		SceneController.Instance.TransitionToMenu();
	}

}
