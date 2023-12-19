using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitable
{
	// calls when entity is hitted.
	void Hit(Stats stats);
	void Hit(float damage);

	Stats GetStats();
}
