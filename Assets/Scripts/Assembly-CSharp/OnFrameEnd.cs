using System;
using UnityEngine;

public class OnFrameEnd : MonoBehaviour
{
	public static OnFrameEnd main;

	public Transform frameTransform;

	public event Action onBeforeFrameEnd_Once;

	public event Action onBeforeFrameEnd_Permanent;

	private void Awake()
	{
		main = this;
	}

	private void Start()
	{
	}

	private void LateUpdate()
	{
		this.onBeforeFrameEnd_Once?.Invoke();
		this.onBeforeFrameEnd_Once = null;
		this.onBeforeFrameEnd_Permanent?.Invoke();
	}
}
