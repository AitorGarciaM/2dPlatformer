using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
	[SerializeField, Min(1f)] private float _scaleMultiplier = 1;
	[SerializeField] private List<ParallaxLayer> _parallaxLayers;
	[SerializeField] private PlayerController _player;

	private List<GameObject> _layers;

    // Start is called before the first frame update
    void Start()
    {
		_layers = new List<GameObject>();

		for(int i = 0; i < _parallaxLayers.Count; i++)
		{
			GameObject tempLayer = new GameObject("Layer", typeof(SpriteRenderer));
			tempLayer.GetComponent<SpriteRenderer>().sprite = _parallaxLayers[i].sprite;
			tempLayer.GetComponent<SpriteRenderer>().sortingLayerName = "Background";
			tempLayer.GetComponent<SpriteRenderer>().sortingOrder = i;

			GameObject tempLayerClone = new GameObject("LayerClone", typeof(SpriteRenderer));
			tempLayerClone.GetComponent<SpriteRenderer>().sprite = _parallaxLayers[i].sprite;
			tempLayerClone.GetComponent<SpriteRenderer>().sortingLayerName = "Background";
			tempLayerClone.GetComponent<SpriteRenderer>().sortingOrder = i;

			tempLayer.transform.SetParent(this.transform);
			tempLayerClone.transform.SetParent(this.transform);

			tempLayer.transform.localScale = new Vector3(_scaleMultiplier, _scaleMultiplier, 1);
			tempLayerClone.transform.localScale = new Vector3(_scaleMultiplier, _scaleMultiplier, 1);

			tempLayerClone.transform.position = new Vector3((tempLayerClone.GetComponent<SpriteRenderer>().sprite.rect.width / 100) * tempLayerClone.transform.localScale.x, 0, 0);

			_layers.Add(tempLayer);
			_layers.Add(tempLayerClone);			
		}

		
	}

    // Update is called once per frame
    void Update()
    {
		for (int i = 0; i < _layers.Count; i++)
		{
			int y = (_layers.Count + i / 2) % _parallaxLayers.Count;

			_layers[i].transform.position += new Vector3(_player.MovementSpeed * _parallaxLayers[y].speedMultiplier, _layers[i].transform.position.y);

			if(_layers[i].transform.position.x > (((_layers[i].GetComponent<SpriteRenderer>().sprite.rect.width / 100) * _layers[i].transform.localScale.x) * 2))
			{
				_layers[i].transform.position = new Vector3(-((_layers[i].GetComponent<SpriteRenderer>().sprite.rect.width / 100) * _layers[i].transform.localScale.x), 0, 0);
			}
			else if(_layers[i].transform.position.x < -(((_layers[i].GetComponent<SpriteRenderer>().sprite.rect.width / 100) * _layers[i].transform.localScale.x) * 2))
			{
				_layers[i].transform.position = new Vector3(((_layers[i].GetComponent<SpriteRenderer>().sprite.rect.width / 100) * _layers[i].transform.localScale.x), 0, 0);
			}
		}
	}
}
