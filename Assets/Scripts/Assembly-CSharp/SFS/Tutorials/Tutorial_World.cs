using System.Linq;
using SFS.Parts.Modules;
using SFS.World;
using UnityEngine;
using UnityEngine.Analytics;

namespace SFS.Tutorials
{
	public class Tutorial_World : Tutorial_Base
	{
		public GameObject usePartPopup;

		public GameObject ignitionPopup;

		public GameObject throttlePopup;

		private void Start()
		{
			usePartPopup.SetActive(value: false);
			ignitionPopup.SetActive(value: false);
			throttlePopup.SetActive(value: false);
			if (FileLocations.HasNotification("Tut_Launch") || AnalyticsSessionInfo.sessionCount > 3)
			{
				return;
			}
			Add_Action(delegate
			{
				usePartPopup.SetActive(value: true);
				ignitionPopup.SetActive(value: true);
				throttlePopup.SetActive(value: true);
			});
			Add_Check(delegate
			{
				if (PlayerController.main.player.Value is Rocket rocket)
				{
					if (rocket.partHolder.GetModules<EngineModule>().Any((EngineModule a) => a.engineOn.Value))
					{
						usePartPopup.SetActive(value: false);
					}
					if (rocket.throttle.throttleOn.Value)
					{
						ignitionPopup.SetActive(value: false);
					}
					if (rocket.throttle.throttlePercent.Value > 0.95f)
					{
						throttlePopup.SetActive(value: false);
					}
				}
				return !usePartPopup.activeSelf && !ignitionPopup.activeSelf && !throttlePopup.activeSelf;
			});
			Add_Action(delegate
			{
				FileLocations.WriteNotification("Tut_Launch");
			});
		}
	}
}
