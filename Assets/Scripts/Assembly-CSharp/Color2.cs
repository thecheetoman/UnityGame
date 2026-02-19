using System;
using UnityEngine;

[Serializable]
public struct Color2
{
	public Color start;

	public Color end;

	public Color2(Color start, Color end)
	{
		this.start = start;
		this.end = end;
	}

	public Color Lerp(float t)
	{
		return Color.Lerp(start, end, t);
	}
}
