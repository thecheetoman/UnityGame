using SFS.Variables;
using UnityEngine;

namespace SFS.Cameras
{
	public class PositionToCamera : MonoBehaviour
	{
		public bool listBased;

		public Transform[] toPosition;

		[Space]
		public Vector2_Local pivot;

		[Space]
		public bool zRelative;

		public Float_Local distance;

		private void LateUpdate()
		{
			float z = (zRelative ? ((float)distance) : (ActiveCamera.Camera.camera.transform.position.z - (float)distance));
			Vector3 position = ActiveCamera.Camera.camera.ScreenToWorldPoint(new Vector3(pivot.Value.x * (float)Screen.width, pivot.Value.y * (float)Screen.height, z));
			if (listBased)
			{
				Transform[] array = toPosition;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].position = position;
				}
			}
			else
			{
				base.transform.position = position;
			}
		}
	}
}
