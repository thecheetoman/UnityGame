using System;
using UnityEngine;

namespace SFS.UI
{
	public class MenuElement
	{
		public readonly Action<GameObject> createElement;

		public MenuElement(Action<GameObject> createElement)
		{
			this.createElement = createElement;
		}
	}
}
