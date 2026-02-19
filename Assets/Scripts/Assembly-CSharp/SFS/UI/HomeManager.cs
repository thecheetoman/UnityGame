using System;
using System.Collections.Generic;
using System.Linq;
using SFS.Audio;
using SFS.Input;
using SFS.Translations;
using UnityEngine;

namespace SFS.UI
{
	public class HomeManager : BasicMenu
	{
		public static HomeManager main;

		public MusicPlaylistPlayer menuMusic;

		public TextAdapter version;

		public Material starsMaterial;

		public ShopManager shopManager;

		public DevelopmentMenu developmentMenu;

		[Space]
		public SaleManager saleManager;

		public BasicMenu redstoneMenu;

		public BasicMenu remoteTriggeredSaleMenu;

		public GameObject steamBanner;

		public Button settingsButton;

		public GameObject fullVersionButton;

		public BasicMenu fullVersionSalePage;

		public GameObject modsMenuButton;

		public string remoteSaleSource;

		private string redstonePackSource;

		private void Awake()
		{
			main = this;
		}

		private void Start()
		{
			modsMenuButton.SetActive(DevSettings.HasModLoader && DevSettings.FullVersion);
			fullVersionButton.SetActive(DevSettings.ShowFullVersionButton);
			RemoteSettings.ForceUpdate();
			menuMusic.StartPlaying(5f);
			starsMaterial.SetColor("_Color", new Color(1f, 1f, 1f, 1f));
			string versionText = GetVersion();
			version.Text = "v" + versionText;
			if (!string.IsNullOrEmpty(Base.sceneLoader.sceneSettings.openShop))
			{
				if (Base.sceneLoader.sceneSettings.openShop == "Full Version")
				{
					fullVersionSalePage.Open();
				}
				return;
			}
			LanguageSettings.main.Initialize(delegate
			{
				ShowUpgradeVersionMenu(versionText, delegate
				{
					ShowIsNewPlayerMenu(delegate
					{
					});
				});
			});
		}

		private static void ShowIsNewPlayerMenu(Action callback)
		{
		}

		private static void ShowUpgradeVersionMenu(string versionText, Action callback)
		{
			callback();
		}

		private static string GetVersion()
		{
			string text = ((Application.version[3] != '.') ? Application.version.Insert(3, ".") : Application.version);
			int num = 313;
			string text2 = ((Application.platform == RuntimePlatform.Android) ? "android" : ((Application.platform == RuntimePlatform.IPhonePlayer) ? "ios" : "other"));
			string[] array = (from a in RemoteSettings.GetString("Version_Display_Data", "").Replace(" ", "").Split(';')
				where a != ""
				select a).ToArray();
			for (int num2 = 0; num2 < array.Length; num2++)
			{
				string[] array2 = array[num2].Split(',');
				string text3 = array2[0];
				int num3 = int.Parse(array2[1]);
				string text4 = array2[2];
				string text5 = array2[3].Replace("_", " ");
				if (text3 == text && num3 == num && (text4 == "any" || text4 == text2))
				{
					return text + ((text5.Length > 0) ? "  -  " : "") + text5;
				}
			}
			return text;
		}

		public void OpenFullVersion()
		{
			fullVersionSalePage.Open();
		}

		public void OpenCommunity()
		{
			SizeSyncerBuilder.Carrier carrier;
			List<MenuElement> list = new List<MenuElement>
			{
				new SizeSyncerBuilder(out carrier).HorizontalMode(SizeMode.MaxChildSize),
				ButtonBuilder.CreateButton(carrier, () => Loc.main.Community_Youtube, delegate
				{
					Application.OpenURL("https://www.youtube.com/channel/UCOpgvpnGyZw4IRT_kuebiWA");
				}, CloseMode.Current).MinSize(300f, 60f),
				ButtonBuilder.CreateButton(carrier, () => Loc.main.Community_Discord, delegate
				{
					Application.OpenURL("https://discordapp.com/invite/hwfWm2d");
				}, CloseMode.Current).MinSize(300f, 60f),
				ButtonBuilder.CreateButton(carrier, () => Loc.main.Community_Reddit, delegate
				{
					Application.OpenURL("https://www.reddit.com/r/SpaceflightSimulator/");
				}, CloseMode.Current).MinSize(300f, 60f),
				ButtonBuilder.CreateButton(carrier, () => Loc.main.Community_Forums, delegate
				{
					Application.OpenURL("https://jmnet.one/sfs/forum/index.php");
				}, CloseMode.Current).MinSize(300f, 60f)
			};
			if (LanguageSettings.main.settings.name == "Russian")
			{
				list.Add(ButtonBuilder.CreateButton(carrier, () => "ВКонтакте", delegate
				{
					Application.OpenURL("https://vk.com/public194508161");
				}, CloseMode.Current).MinSize(300f, 60f));
			}
			ScreenManager.main.OpenScreen(MenuGenerator.CreateMenu(CancelButton.Close, CloseMode.Current, delegate
			{
			}, delegate
			{
			}, list.ToArray()));
		}

		public void OpenSettings()
		{
			Menu.settings.Open();
		}

		public void OpenCredits()
		{
			Menu.read.Open(() => Loc.main.Credits_Text, CloseMode.Current, background: false);
		}

		public void OpenPC()
		{
			Application.OpenURL("https://spaceflight-simulator-steam.azurewebsites.net/");
		}

		public static void OpenTutorials_Static()
		{
			SizeSyncerBuilder.Carrier carrier;
			List<MenuElement> list = new List<MenuElement>
			{
				new SizeSyncerBuilder(out carrier).HorizontalMode(SizeMode.MaxChildSize),
				ButtonBuilder.CreateButton(carrier, () => Loc.main.Video_Orbit, delegate
				{
					Application.OpenURL("https://youtu.be/5uorANMdB60");
				}, CloseMode.Current).MinSize(300f, 60f),
				ButtonBuilder.CreateButton(carrier, () => Loc.main.Video_Moon, delegate
				{
					Application.OpenURL("https://youtu.be/bMv5LmSNgdo");
				}, CloseMode.Current).MinSize(300f, 60f),
				ButtonBuilder.CreateButton(carrier, () => Loc.main.Video_Dock, delegate
				{
					Application.OpenURL("https://youtu.be/PkW87qJYEzg");
				}, CloseMode.Current).MinSize(300f, 60f)
			};
			ScreenManager.main.OpenScreen(MenuGenerator.CreateMenu(CancelButton.Close, CloseMode.Current, delegate
			{
			}, delegate
			{
			}, list.ToArray()));
		}

		public override void Close()
		{
			MenuGenerator.OpenConfirmation(CloseMode.Current, () => Loc.main.Close_Game, () => Loc.main.Close, Application.Quit);
		}
	}
}
