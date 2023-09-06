using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aitor
{
	namespace BehaivourTree
	{
		public class Selector : Node
		{
			public Selector() : base() { }
			public Selector(List<Node> children) : base(children) { }

			public override NodeState Evaluate()
			{
				foreach (Node child in children)
				{
					switch (child.Evaluate())
					{
						case NodeState.FAILURE:
							continue;
						case NodeState.RUNNING:
							return nodeState;
						case NodeState.SUCCESS:
							nodeState = NodeState.SUCCESS;
							return nodeState;
						default:
							continue;
					}
				}

				nodeState = NodeState.FAILURE;

				return nodeState;
			}
		}
	}
}
