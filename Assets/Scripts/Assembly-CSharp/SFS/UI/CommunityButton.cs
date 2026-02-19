using System;
using SFS.Translations;
using UnityEngine;

namespace SFS.UI
{
	public class CommunityButton : MonoBehaviour
	{
		public string communityLink;

		public bool enableOnTranslation;

		public string translationName;

		private void Start()
		{
			if (enableOnTranslation)
			{
				Base.language.RunAfterInitialization(CheckActive);
				LanguageSettings.main.onChange += new Action(CheckActive);
			}
		}

		private void OnDestroy()
		{
			LanguageSettings.main.onChange -= new Action(CheckActive);
		}

		private void CheckActive()
		{
			base.gameObject.SetActive(LanguageSettings.main.settings.name == translationName);
		}

		public void OpenCommunity()
		{
			Application.OpenURL(communityLink);
		}
	}
}
