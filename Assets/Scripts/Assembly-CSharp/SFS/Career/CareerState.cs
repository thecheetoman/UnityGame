using System;
using System.Collections.Generic;
using SFS.Audio;
using SFS.Builds;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.UI;
using SFS.World;
using SFS.WorldBase;
using UnityEngine;
using UnityEngine.Analytics;

namespace SFS.Career
{
	public class CareerState : MonoBehaviour
	{
		public static CareerState main;

		public WorldSave.CareerState state { get; private set; }

		private bool CompletedTechTree
		{
			get
			{
				if (HasFeature(WorldSave.CareerState.completedTree_1))
				{
					return HasFeature(WorldSave.CareerState.completedTree_2);
				}
				return false;
			}
		}

		public event Action OnFundsChange;

		public event Action OnStateLoaded;

		private void Awake()
		{
			main = this;
		}

		private void Start()
		{
			if (BuildManager.main != null)
			{
				SetState(SavingCache.main.LoadWorldPersistent(MsgDrawer.main, needsRocketsAndBranches: false, eraseCache: false).career);
			}
		}

		public void SetState(WorldSave.CareerState state)
		{
			this.state = state;
			this.OnStateLoaded?.Invoke();
		}

		public bool HasPart(VariantRef a)
		{
			if (Base.worldBase.SandboxMode)
			{
				if (Base.worldBase.settings.mode.mode == WorldMode.Mode.Classic)
				{
					return a.part.GetModuleCount<CareerOnlyPart>() == 0;
				}
				return true;
			}
			if (CompletedTechTree)
			{
				return true;
			}
			VariantRef[] parts = ResourcesLoader.main.partPacks["Basics"].parts;
			foreach (VariantRef variantRef in parts)
			{
				if (a.GetVariant() == variantRef.GetVariant())
				{
					return true;
				}
			}
			if (state.unlocked_Parts.Contains(a.GetNameID()))
			{
				return true;
			}
			foreach (string unlocked_Upgrade in state.unlocked_Upgrades)
			{
				if (!ResourcesLoader.main.partPacks.TryGetValue(unlocked_Upgrade, out var value))
				{
					continue;
				}
				parts = value.parts;
				foreach (VariantRef variantRef2 in parts)
				{
					if (a.GetVariant() == variantRef2.GetVariant())
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool HasPart(Part part)
		{
			if (Base.worldBase.SandboxMode)
			{
				if (Base.worldBase.settings.mode.mode == WorldMode.Mode.Classic)
				{
					return part.GetModuleCount<CareerOnlyPart>() == 0;
				}
				return true;
			}
			if (CompletedTechTree)
			{
				return true;
			}
			VariantRef[] parts = ResourcesLoader.main.partPacks["Basics"].parts;
			for (int i = 0; i < parts.Length; i++)
			{
				if (parts[i].part.name == part.name)
				{
					return true;
				}
			}
			if (part.name == "CapsuleNew")
			{
				return true;
			}
			foreach (string unlocked_Part in state.unlocked_Parts)
			{
				if (Base.partsLoader.partVariants[unlocked_Part].part.name == part.name)
				{
					return true;
				}
			}
			foreach (string unlocked_Upgrade in state.unlocked_Upgrades)
			{
				if (!ResourcesLoader.main.partPacks.TryGetValue(unlocked_Upgrade, out var value))
				{
					continue;
				}
				parts = value.parts;
				for (int i = 0; i < parts.Length; i++)
				{
					if (parts[i].part.name == part.name)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool HasUpgrade(string name_ID)
		{
			if (Base.worldBase.SandboxMode)
			{
				return true;
			}
			return state.unlocked_Upgrades.Contains(name_ID);
		}

		public bool HasFeature((WorldSave.UnlockTarget, string) a)
		{
			if (Base.worldBase.SandboxMode)
			{
				return true;
			}
			return a.Item1 switch
			{
				WorldSave.UnlockTarget.UnlockedPart => state.unlocked_Parts.Contains(a.Item2), 
				WorldSave.UnlockTarget.UnlockedUpgrade => state.unlocked_Upgrades.Contains(a.Item2), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}

		public void UnlockPart(VariantRef part)
		{
			AddToList(state.unlocked_Parts, part.GetNameID());
			AnalyticsEvent.Custom("Unlock_" + part.GetNameID(), new Dictionary<string, object>());
		}

		public void UnlockUpgrade(string upgrade_CodeName)
		{
			AddToList(state.unlocked_Upgrades, upgrade_CodeName);
			AnalyticsEvent.Custom("Unlock_" + upgrade_CodeName, new Dictionary<string, object>());
		}

		public void MarkInfoAsRead(string info_CodeName)
		{
			throw new NotImplementedException();
		}

		private static void AddToList(List<string> unlockedList, string codeName)
		{
			if (!unlockedList.Contains(codeName))
			{
				unlockedList.Add(codeName);
			}
		}

		public void TryBuy(double cost, Action onBuy)
		{
			if (!CanBuy(cost))
			{
				MsgDrawer.main.Log("Insufficient funds");
				SoundPlayer.main.denySound.Play();
			}
			else
			{
				TakeFunds(cost);
				onBuy?.Invoke();
			}
		}

		private bool CanBuy(double cost)
		{
			return cost <= state.funds;
		}

		public void RewardFunds(double a)
		{
			state.funds += a;
			this.OnFundsChange?.Invoke();
		}

		private void TakeFunds(double a)
		{
			state.funds -= a;
			this.OnFundsChange();
		}
	}
}
