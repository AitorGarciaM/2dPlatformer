using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFightController : MonoBehaviour
{
	[SerializeField] Doors _doors;
	[SerializeField] BossController _astaroth;
	[SerializeField] BoxCollider2D _trigger;

	[SerializeField] LayerMask _playerMask;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		Collider2D playerCollider = Physics2D.OverlapBox(transform.position + (Vector3)_trigger.offset, _trigger.size, 0, _playerMask);

		if(playerCollider != null)
		{
			_doors.StartBoosFight();
			Invoke("SummonBoss", 1);
		}
    }

	private void SummonBoss()
	{
		_astaroth.gameObject.SetActive(true);
	}
}
