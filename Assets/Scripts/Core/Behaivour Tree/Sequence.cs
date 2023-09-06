using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aitor
{
	namespace BehaivourTree
	{
		public class Sequence : Node
		{
			public Sequence() : base() {}
			public Sequence(List<Node> children) : base(children) {}

			public override NodeState Evaluate()
			{
				bool isAnyChildRunning = false;

				foreach(Node child in children)
				{
					switch(child.Evaluate())
					{
						case NodeState.FAILURE:
							nodeState = NodeState.FAILURE;
							return nodeState;
						case NodeState.RUNNING:
							isAnyChildRunning = true;
							continue;
						case NodeState.SUCCESS:
							nodeState = NodeState.SUCCESS;
							continue;
						default:
							nodeState = NodeState.SUCCESS;
							return nodeState;
					}
				}

				nodeState = isAnyChildRunning ? NodeState.RUNNING : NodeState.SUCCESS;
				return nodeState;
			}
		}
	}
}
