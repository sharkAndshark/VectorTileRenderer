using System;

namespace ClipperLib
{
	internal class IntersectNode
	{
		public TEdge edge1;

		public TEdge edge2;

		public IntPoint pt;

		public IntersectNode next;

		public IntersectNode()
		{
		}
	}
}