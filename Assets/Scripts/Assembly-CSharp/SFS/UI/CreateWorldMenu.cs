using System;
using System.Collections.Generic;
using System.Globalization;
using SFS.IO;
using SFS.Input;
using SFS.Translations;
using SFS.World;
using SFS.WorldBase;
using UnityEngine;

namespace SFS.UI
{
	public class CreateWorldMenu : BasicMenu
	{
		public WorldsMenu worldsMenu;

		public Button createWorldButton;

		public TextBoxAdapter worldNameField;

		public TextAdapter solarSystemButtonText;

		public ButtonGroup difficultyGroup;

		public ButtonGroup modeGroup;

		public GameObject sandboxPurchasePopup;

		public TextAdapter difficultyInfo;

		public TextAdapter modeInfo;

		private static readonly List<WorldMode.Mode> ModeIndices = new List<WorldMode.Mode>
		{
			WorldMode.Mode.Classic,
			WorldMode.Mode.Career,
			WorldMode.Mode.Sandbox
		};

		private static readonly List<Difficulty> DifficultyIndices = new List<Difficulty>
		{
			Difficulty.Normal,
			Difficulty.Hard,
			Difficulty.Realistic
		};

		private string solarSystemName;

		public override void Open()
		{
			worldNameField.Text = Loc.main.Default_World_Name;
			SetSolarSystem("");
			modeGroup.SetSelectedIndex(0);
			difficultyGroup.SetSelectedIndex(0);
			base.Open();
		}

		public override void Close()
		{
			worldNameField.Close();
			base.Close();
		}

		private void SetSolarSystem(string a)
		{
			solarSystemName = a;
			solarSystemButtonText.Text = ((a == "") ? ((string)Loc.main.Default_Solar_System) : a);
		}

		public void OpenSelectSolarSystemMenu()
		{
			SizeSyncerBuilder.Carrier horizontal;
			List<MenuElement> buttons = new List<MenuElement>
			{
				TextBuilder.CreateText().Text(() => Loc.main.Select_Solar_System),
				ElementGenerator.VerticalSpace(50),
				new SizeSyncerBuilder(out horizontal)
			};
			AddButton(Loc.main.Default_Solar_System, "");
			if (DevSettings.FullVersion && FileLocations.SolarSystemsFolder.FolderExists())
			{
				foreach (FolderPath item in FileLocations.SolarSystemsFolder.GetFoldersInFolder(recursively: false))
				{
					if (item.FolderName != "Example")
					{
						AddButton(Loc.main.Custom_Solar_System.Inject(item.FolderName, "name"), item.FolderName);
					}
				}
			}
			MenuGenerator.OpenMenu(CancelButton.Cancel, CloseMode.Current, buttons.ToArray());
			void AddButton(string text, string solarSystemName)
			{
				buttons.Add(ButtonBuilder.CreateButton(horizontal, () => text, delegate
				{
					SetSolarSystem(solarSystemName);
				}, CloseMode.Current));
			}
		}

		public void OnDifficultyCycle()
		{
			Difficulty difficulty = DifficultyIndices[difficultyGroup.SelectedIndex];
			string text = "";
			text += Loc.main.Difficulty_Scale_Stat.Inject((20.0 / difficulty.DefaultRadiusScale).ToString(CultureInfo.InvariantCulture), "scale");
			text = text + "\n" + Loc.main.Difficulty_Isp_Stat.Inject(difficulty.IspMultiplier.ToString(CultureInfo.InvariantCulture), "value");
			text = text + "\n" + Loc.main.Difficulty_Dry_Mass_Stat.Inject(difficulty.DryMassMultiplier.ToString(CultureInfo.InvariantCulture), "value");
			text = text + "\n" + Loc.main.Difficulty_Engine_Mass_Stat.Inject(difficulty.EngineMassMultiplier.ToString(CultureInfo.InvariantCulture), "value");
			difficultyInfo.Text = text;
		}

		public void OnModeCycle()
		{
			string text = "";
			switch (ModeIndices[modeGroup.SelectedIndex])
			{
			case WorldMode.Mode.Classic:
				text = "";
				break;
			case WorldMode.Mode.Career:
				text = "Coming soon!";
				break;
			case WorldMode.Mode.Sandbox:
				text = "Coming soon!";
				break;
			}
			modeInfo.Text = text;
			createWorldButton.SetEnabled(modeGroup.SelectedIndex == 0);
		}

		public void CreateWorld()
		{
			if (!FileLocations.WorldsFolder.FolderExists())
			{
				FileLocations.WorldsFolder.CreateFolder();
			}
			string text = ((worldNameField.Text == "") ? ((string)Loc.main.Default_World_Name) : worldNameField.Text);
			text = PathUtility.MakeUsable(text, Loc.main.Default_World_Name);
			text = PathUtility.AutoNameExisting(text, FileLocations.WorldsFolder);
			WorldReference worldReference = new WorldReference(text);
			SolarSystemReference solarSystem = new SolarSystemReference(solarSystemName);
			Difficulty difficulty = DifficultyIndices[difficultyGroup.SelectedIndex];
			bool flag = !(text.ToLower() == "Test Career".ToLower()) && false;
			WorldMode.Mode mode = (flag ? WorldMode.Mode.Career : WorldMode.Mode.Classic);
			WorldSettings settings = new WorldSettings(solarSystem, difficulty, new WorldMode(mode), new WorldPlaytime(DateTime.Now.Ticks, 0.0), new SandboxSettings.Data());
			worldReference.SaveWorldSettings(settings);
			Close();
			worldsMenu.OnWorldCreated(text);
			if (flag)
			{
				Menu.read.Open(() => "Career mode is currently a very minimal prototype with only a few missions and unlocks\n\nIf this prototype gets positive feedback, we will add tons of missions, unlocks and polish it to perfection\n\n- Spaceflight Simulator developer team");
			}
		}
	}
}
