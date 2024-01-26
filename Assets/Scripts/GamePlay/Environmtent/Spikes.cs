using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
	[SerializeField] private BoxCollider2D _hitArea;
	[SerializeField] private float _damage;
	[SerializeField] private LayerMask _hitMask;

    // Start is called before the first frame update
    void Start()
    {
        
    }

	private void FixedUpdate()
	{
		// Check if hitable is hitted.
		Collider2D[] colliders = Physics2D.OverlapBoxAll(_hitArea.transform.position, _hitArea.size, 0, _hitMask);
		if (colliders.Length != 0)
		{
			foreach(var collider in colliders)
			{
				IHitable hitable;

				if(collider.gameObject.TryGetComponent<IHitable>(out hitable))
				{
					hitable.Hit(_damage);

					PlayerController player = (PlayerController)hitable;

					if(player != null)
					{
						if (!player.ResetingPosition && !player.IsDead)
						{
							player.ResetPosition();
						}
					}
				}
			}
		}
	}
}
