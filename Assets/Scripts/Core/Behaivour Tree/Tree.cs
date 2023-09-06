using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aitor
{
	namespace BehaivourTree
	{
		public abstract class Tree : MonoBehaviour
		{
			protected Node _root;

			protected virtual void Start()
			{
				_root = SetUpTree();
			}

			// Update is called once per frame
			protected virtual void Update()
			{
				if(_root != null)
				{
					_root.Evaluate();
				}
			}

			protected abstract Node SetUpTree();
		}
	}
}