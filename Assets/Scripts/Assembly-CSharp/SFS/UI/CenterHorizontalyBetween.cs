using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SFS.UI
{
	[RequireComponent(typeof(RectTransform))]
	public class CenterHorizontalyBetween : MonoBehaviour
	{
		public RectTransform self;

		public RectTransform[] left;

		public RectTransform[] right;

		private void LateUpdate()
		{
			Reposition();
		}

		private void Reposition()
		{
			IEnumerable<RectTransform> source = left.Where((RectTransform rect) => rect.gameObject.activeInHierarchy);
			IEnumerable<RectTransform> source2 = right.Where((RectTransform rect) => rect.gameObject.activeInHierarchy);
			if (source.Count() != 0 && source2.Count() != 0)
			{
				float num = source.Max((RectTransform rect) => rect.GetWorldPosition(Vector2.one).x);
				float num2 = source2.Min((RectTransform rect) => rect.GetWorldPosition(Vector2.zero).x) - num;
				Vector2 vector = self.position;
				vector.x = num + num2 * 0.5f - (0.5f - self.pivot.x) * (self.rect.width * self.lossyScale.x);
				self.position = vector;
			}
		}
	}
}
