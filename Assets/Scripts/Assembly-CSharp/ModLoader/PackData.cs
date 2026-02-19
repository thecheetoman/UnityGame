using UnityEngine;

namespace ModLoader
{
	public class PackData : ScriptableObject
	{
		public string DisplayName;

		public string Version;

		public string Description;

		public string Author;

		public bool ShowIcon;

		public Texture2D Icon;
	}
}
