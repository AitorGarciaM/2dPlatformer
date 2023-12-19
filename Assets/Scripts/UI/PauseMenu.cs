using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
	[SerializeField] GameObject _firstSelected;

    // Start is called before the first frame update
    void Start()
    {
		EventSystem.current.SetSelectedGameObject(_firstSelected);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
