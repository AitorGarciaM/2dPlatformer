using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : TransitionDestination
{
   public void SetCheckPoint()
	{
		SceneController.Instance.SetCheckPoint(this);
	}
}
