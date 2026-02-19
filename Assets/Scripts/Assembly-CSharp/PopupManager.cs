using System;
using System.Collections.Generic;
using SFS;
using SFS.Builds;
using SFS.Parsers.Json;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
	[Serializable]
	public class Popup
	{
		public GameObject popup;

		public Feature type;
	}

	[Serializable]
	public class State
	{
		public Dictionary<Feature, FeatureTracker> features = new Dictionary<Feature, FeatureTracker>();
	}

	[Serializable]
	public class FeatureTracker
	{
		public int timesUsed;
	}

	public enum Feature
	{
		Symmetry = 0,
		Stages = 1,
		AreaSelect = 2,
		DoubleClickSelect = 3
	}

	public static PopupManager main;

	public Popup[] popups;

	private State state;

	private bool started;

	private void Awake()
	{
		main = this;
	}

	private void Start()
	{
		if (!started)
		{
			Load();
			SceneLoader sceneLoader = Base.sceneLoader;
			sceneLoader.onSceneUnload_Once = (Action)Delegate.Combine(sceneLoader.onSceneUnload_Once, new Action(Save));
			started = true;
			Popup[] array = popups;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].popup.SetActive(value: false);
			}
			if (BuildManager.main != null)
			{
				Initialize();
			}
		}
	}

	public void Initialize()
	{
		if (!started)
		{
			Start();
		}
		if (RemoteSettings.GetBool("Tut_Build_Popups", defaultValue: true) && BuildManager.main != null && !TryShow(Feature.Symmetry, 0.5f) && !TryShow(Feature.Stages, 1.5f) && !TryShow(Feature.AreaSelect, 2f))
		{
			TryShow(Feature.DoubleClickSelect, 2.5f);
		}
	}

	private bool TryShow(Feature type, float playtimeHours, bool markUsed = true)
	{
		if ((float)PlaytimeTracker.GetActivePlaytimeSeconds() < playtimeHours)
		{
			return false;
		}
		if (state.features.ContainsKey(type) && state.features[type].timesUsed > 0)
		{
			return false;
		}
		Popup[] array = popups;
		foreach (Popup popup in array)
		{
			if (popup.type == type)
			{
				popup.popup.SetActive(value: true);
				if (markUsed)
				{
					MarkUsed(type, disablePopup: false);
				}
				return true;
			}
		}
		return false;
	}

	public static void MarkUsed(Feature feature, bool disablePopup = true)
	{
	}

	private void Save()
	{
		FileLocations.GetNotificationsPath("Feature_Use").WriteText(JsonWrapper.ToJson(state, pretty: true));
	}

	private void Load()
	{
		state = (JsonWrapper.TryLoadJson<State>(FileLocations.GetNotificationsPath("Feature_Use"), out var data) ? data : new State());
	}
}
