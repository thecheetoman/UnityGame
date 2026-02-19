using SFS.Cameras;
using UnityEngine;

namespace SFS.UI
{
	public class AttachToTransform : MonoBehaviour
	{
		public Transform target;

		[Space]
		public Canvas canvas;

		public Vector2 localPosition;

		public Vector2 offsetUI;

		private void LateUpdate()
		{
			base.transform.localPosition = ActiveCamera.Camera.camera.WorldToScreenPoint(target.TransformPoint(localPosition)) / canvas.scaleFactor + (Vector3)offsetUI;
		}
	}
}
