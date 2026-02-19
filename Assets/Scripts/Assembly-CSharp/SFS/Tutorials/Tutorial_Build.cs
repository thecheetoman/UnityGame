using SFS.Builds;
using UnityEngine;
using UnityEngine.Analytics;

namespace SFS.Tutorials
{
	public class Tutorial_Build : Tutorial_Base
	{
		public GameObject dragAndDropPopup;

		public GameObject descriptionPopup;

		public GameObject exampleRocketsPopup;

		public GameObject infiniteArea;

		private void Start()
		{
			dragAndDropPopup.SetActive(value: false);
			descriptionPopup.SetActive(value: false);
			exampleRocketsPopup.SetActive(value: false);
			infiniteArea.SetActive(value: false);
			if (FileLocations.HasNotification("Tut_Basic_Build"))
			{
				return;
			}
			if (AnalyticsSessionInfo.sessionCount > 3)
			{
				return;
			}
			Add_ShowPopup(dragAndDropPopup, () => BuildManager.main.holdGrid.HasParts(includePreHeld: true));
			Add_Check(() => BuildState.main.buildGrid.activeGrid.partsHolder.parts.Count > 0);
			bool seenDescription = false;
			BuildManager.main.pickGrid.OnPartMenuOpen += delegate
			{
				seenDescription = true;
			};
			Add_ShowPopup(descriptionPopup, () => seenDescription);
			Add_Action(delegate
			{
				FileLocations.WriteNotification("Tut_Basic_Build");
			});
			if (RemoteSettings.GetBool("Tut_ExampleRockets", defaultValue: true))
			{
				Add_Action(delegate
				{
					exampleRocketsPopup.SetActive(value: true);
				});
			}
		}
	}
}
