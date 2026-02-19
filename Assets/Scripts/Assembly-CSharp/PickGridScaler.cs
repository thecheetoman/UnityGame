using UnityEngine;

public class PickGridScaler : MonoBehaviour
{
	public RectTransform holder;

	private void LateUpdate()
	{
		float num = holder.rect.height / GetComponent<RectTransform>().sizeDelta.y;
		base.transform.localScale = Vector2.one * num;
	}
}
