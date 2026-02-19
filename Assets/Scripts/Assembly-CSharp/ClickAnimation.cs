using System;
using SFS.UI;
using UnityEngine;

public class ClickAnimation : MonoBehaviour
{
	public Button button;

	public AnimationCurve curve;

	private Vector3 size;

	private float clickTime;

	private void Start()
	{
		size = base.transform.localScale;
		button.onClick += new Action(OnClick);
	}

	private void OnClick()
	{
		clickTime = Time.unscaledTime;
	}

	private void Update()
	{
		base.transform.localScale = size * curve.Evaluate(Time.unscaledTime - clickTime);
	}
}
