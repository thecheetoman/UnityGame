using System;
using System.Collections.Generic;
using SFS.Achievements;
using SFS.Cameras;
using SFS.Core;
using SFS.Input;
using SFS.Translations;
using SFS.Tween;
using SFS.UI;
using SFS.World;
using SFS.WorldBase;
using UnityEngine;

namespace SFS.Career
{
	public class HubManager : MonoBehaviour
	{
		public static HubManager main;

		public TextAdapter[] fundsTexts;

		public GameObject funds;

		[Space]
		public SpaceCenter spaceCenter;

		public CameraManager worldCamera;

		public WorldEnvironment environment;

		public PostProcessing postProcessing;

		[Space]
		public Screen_Game hub_Input;

		[Space]
		public Button resumeGameButton;

		[Space]
		public TextAdapter researchButtonText;

		public BasicMenu researchTab;

		public TreeComponent tree_Research;

		[Space]
		public TextAdapter achievementsButtonText;

		public BasicMenu achievementsTab;

		public ScrollElement achievementsScroller;

		public RectTransform planetTitlePrefab;

		public RectTransform planetAchievementsPrefab;

		public VerticalLayout planetAchievementsLayout;

		[Space]
		public GameObject tutorialsButton;

		public GameObject astronautsButton;

		[Space]
		private WorldSave state;

		public HubManager()
		{
			main = this;
		}

		private void Start()
		{
			ActiveCamera.Camera = worldCamera;
			LoadPersistent();
			SavingCache.main.Preload_BlueprintPersistent();
			SavingCache.main.Preload_WorldPersistent(needsRocketsAndBranches: true);
			TimeEvent timeEvent = TimeEvent.main;
			timeEvent.on_10000Ms = (Action)Delegate.Combine(timeEvent.on_10000Ms, new Action(UpdatePersistent));
			funds.SetActive(Base.worldBase.IsCareer);
			resumeGameButton.SetEnabled(Base.worldBase.paths.CanResumeGame());
			tutorialsButton.SetActive(value: false);
			var (num, num2) = AchievementsModule.DrawAllAchievements(state.achievements, CreatePlanetElement);
			achievementsButtonText.Text = Loc.main.Achievements_Button.Inject(num.ToString(), "complete").Inject(num2.ToString(), "total");
			Location value = spaceCenter.vab.building.location.Value;
			value.position.y += 75.0;
			WorldView.main.SetViewLocation(value);
			environment.Initialize(launchPlanetOnly: true);
			hub_Input.keysNode.AddOnKeyDown(KeybindingsPC.keys.Close_Menu, OpenMenu);
			RectTransform Create(RectTransform prefab)
			{
				RectTransform rectTransform = UnityEngine.Object.Instantiate(prefab, achievementsScroller.transform);
				rectTransform.gameObject.SetActive(value: true);
				if (planetAchievementsLayout != null)
				{
					planetAchievementsLayout.AddElement(rectTransform.GetComponent<NewElement>());
				}
				return rectTransform;
			}
			void CreatePlanetElement(string title, Planet planet, List<string> achievements)
			{
				RectTransform planetTitle = Create(planetTitlePrefab);
				planetTitle.GetComponentInChildren<TextAdapter>().Text = title;
				achievementsScroller.RegisterScrolling(planetTitle.GetComponent<Button>());
				RectTransform[] elements = new RectTransform[achievements.Count];
				for (int i = 0; i < achievements.Count; i++)
				{
					elements[i] = Create(planetAchievementsPrefab);
					elements[i].GetComponent<TextAdapter>().Text = achievements[i];
					elements[i].gameObject.SetActive(value: false);
				}
				bool active = planet.codeName == "Earth" || planet.codeName == "Moon";
				Apply();
				planetTitle.GetComponent<Button>().onClick += (Action)delegate
				{
					active = !active;
					Apply();
				};
				void Apply()
				{
					int index = 4;
					planetTitle.GetChild(index).TweenLocalRotate(new Vector3(0f, 0f, (!active) ? 90 : 0), 0.15f, setValueToTargetOnKill: true);
					RectTransform[] array = elements;
					for (int j = 0; j < array.Length; j++)
					{
						array[j].gameObject.SetActive(active);
					}
				}
			}
		}

		private void OnDestroy()
		{
			TimeEvent timeEvent = TimeEvent.main;
			timeEvent.on_10000Ms = (Action)Delegate.Remove(timeEvent.on_10000Ms, new Action(UpdatePersistent));
		}

		public void OpenMenu()
		{
			MenuGenerator.OpenMenu(CancelButton.Close, CloseMode.Current, new SizeSyncerBuilder(out var carrier).HorizontalMode(SizeMode.MaxChildSize), ButtonBuilder.CreateButton(carrier, () => Loc.main.Open_Cheats_Button, SandboxSettings.main.Open, CloseMode.None), ButtonBuilder.CreateButton(carrier, () => Loc.main.Open_Settings_Button, Menu.settings.Open, CloseMode.None), ElementGenerator.VerticalSpace(10), ButtonBuilder.CreateButton(carrier, () => Loc.main.Resume_Game, ResumeGame, CloseMode.None).CustomizeButton(delegate(Button b)
			{
				b.SetEnabled(Base.worldBase.paths.CanResumeGame());
			}), ButtonBuilder.CreateButton(carrier, () => Loc.main.Exit_To_Main_Menu, ExitToMainMenu, CloseMode.None));
		}

		public void ExitToMainMenu()
		{
			UpdatePersistent();
			Base.sceneLoader.LoadHomeScene(null);
		}

		public void EnterBuild()
		{
			if (Base.worldBase.IsCareer && CareerState.main.state.funds > 0.0 && CareerState.main.state.unlocked_Upgrades.Count == 0)
			{
				MsgDrawer.main.Log("New part unlock is available");
				return;
			}
			UpdatePersistent();
			Base.sceneLoader.LoadBuildScene(askBuildNew: true);
		}

		public void ResumeGame()
		{
			UpdatePersistent();
			Base.sceneLoader.LoadWorldScene();
		}

		public void OpenResearch()
		{
			researchTab.Open();
		}

		public void OpenAchievements()
		{
			achievementsTab.Open();
		}

		public void OpenAstronauts()
		{
			AstronautMenu.main.OpenMenu(null, null);
		}

		public void OpenTutorials()
		{
			HomeManager.OpenTutorials_Static();
		}

		public void CloseResearch()
		{
			if (Base.worldBase.IsCareer && CareerState.main.state.funds > 0.0 && CareerState.main.state.unlocked_Upgrades.Count == 0)
			{
				MsgDrawer.main.Log("New part unlock is available");
			}
			else
			{
				researchTab.Close();
			}
		}

		public void UpdateCareerProgressText()
		{
			var (num, num2) = TT_Creator.GetProgress(tree_Research);
			researchButtonText.Text = Loc.main.Research_And_Development.Inject((num - 4).ToString(), "complete").Inject((num2 - 4).ToString(), "total");
		}

		private void LoadPersistent()
		{
			MsgCollector logger = new MsgCollector();
			state = SavingCache.main.LoadWorldPersistent(logger, needsRocketsAndBranches: false, eraseCache: false);
			CareerState.main.SetState(state.career);
			AstronautState.main.state = state.astronauts;
			if (logger.msg.Length > 0)
			{
				ActionQueue.main.QueueMenu(Menu.read.Create(() => logger.msg.ToString(), delegate
				{
				}, delegate
				{
				}, CloseMode.Current));
			}
		}

		private void UpdatePersistent()
		{
			state.version = Application.version;
			SavingCache.main.SaveWorldPersistent(state, cache: true, saveRocketsAndBranches: false, addToRevert: false, deleteRevert: false);
		}
	}
}
