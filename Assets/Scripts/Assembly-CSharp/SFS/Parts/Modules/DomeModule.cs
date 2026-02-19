using System;
using System.Collections.Generic;
using System.Linq;
using SFS.Variables;
using UnityEngine;

namespace SFS.Parts.Modules
{
	public class DomeModule : MonoBehaviour
	{
		[Serializable]
		public class Trigger
		{
			public Composed_Vector2 position;

			public Composed_Vector2 normal;

			public (Vector3, Vector2) GetWorld(DomeModule owner)
			{
				return (owner.transform.TransformPoint(position.Value), owner.transform.TransformVectorUnscaled(normal.Value));
			}
		}

		[Serializable]
		public class Point
		{
			public Composed_Vector2 position;

			public Composed_Vector2 normal;

			public MeshRenderer dome;
		}

		[SerializeField]
		private Trigger[] triggers;

		[SerializeField]
		private Point[] points;

		public static void UpdateInteraction(params Part[] parts)
		{
		}

		private void Adapt(List<(Vector3 position, Vector2 normal)> triggerWorldPositions)
		{
			Point[] array = points;
			foreach (Point point in array)
			{
				Vector3 pointPosition = base.transform.TransformPoint(point.position.Value);
				Vector2 pointNormal = base.transform.TransformVectorUnscaled(point.normal.Value);
				point.dome.enabled = !triggerWorldPositions.Any(((Vector3 position, Vector2 normal) trigger) => (trigger.position - pointPosition).sqrMagnitude < 0.01f && (pointNormal + trigger.normal).sqrMagnitude < 0.01f);
			}
		}
	}
}
