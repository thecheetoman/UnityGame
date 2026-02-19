using System;
using System.Collections.Generic;
using SFS.IO;

namespace ModLoader
{
	public class ModsSettings : SettingsBase<ModsSettings.Data>
	{
		[Serializable]
		public class Data
		{
			public Dictionary<string, bool> modsActive = new Dictionary<string, bool>();

			public Dictionary<string, bool> assetPacksActive = new Dictionary<string, bool>();

			public Dictionary<string, bool> texturePacksActive = new Dictionary<string, bool>();
		}

		public static ModsSettings main;

		protected override string FileName => "ModsSettings";

		private void Awake()
		{
			main = this;
			Load();
		}

		public void SaveAll()
		{
			Save();
		}

		protected override void OnLoad()
		{
		}
	}
}
