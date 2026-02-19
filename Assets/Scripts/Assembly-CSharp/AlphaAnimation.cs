using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AlphaAnimation : MonoBehaviour
{
	public Image image;

	public AnimationCurve curve;

	private float t;

	private int direction;

	private void OnEnable()
	{
		RunForwards();
	}

	public void RunForwards()
	{
		base.enabled = true;
		direction = 1;
		t = 0f;
	}

	public void RunBackwards()
	{
		base.enabled = true;
		direction = -1;
		t = curve.keys.Last().time;
	}

	private void Update()
	{
		t += (float)direction * Time.unscaledDeltaTime;
		image.color = new Color(image.color.r, image.color.g, image.color.b, curve.Evaluate(t));
		if (t < 0f)
		{
			base.gameObject.SetActive(value: false);
		}
		if (t > curve.keys.Last().time)
		{
			base.enabled = false;
		}
	}
}
