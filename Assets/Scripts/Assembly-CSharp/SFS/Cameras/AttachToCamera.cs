using System;
using UnityEngine;

namespace SFS.Cameras
{
	public abstract class AttachToCamera : MonoBehaviour
	{
		protected virtual void OnEnable()
		{
			ActiveCamera.main.activeCamera.OnChange += new Action<CameraManager, CameraManager>(OnCameraChange);
		}

		protected virtual void OnDisable()
		{
			ActiveCamera.main.activeCamera.OnChange -= new Action<CameraManager, CameraManager>(OnCameraChange);
		}

		protected abstract void OnCameraChange(CameraManager oldManager, CameraManager newManager);
	}
}
