using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthRestorerer : MonoBehaviour
{
	private PlayerController _player;

	public void RestoreHealth()
	{
		if (_player != null)
		{
			_player.ResetHealth();
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if(collision.tag == "Player")
		{
			_player = collision.GetComponent<PlayerController>();
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.tag == "Player")
			{
				_player = null;
			}
	}
}
