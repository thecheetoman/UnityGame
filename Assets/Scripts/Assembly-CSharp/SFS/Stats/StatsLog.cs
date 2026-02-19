using System.Collections.Generic;
using System.Linq;
using SFS.Achievements;
using SFS.World;
using UnityEngine;

namespace SFS.Stats
{
	public class StatsLog : MonoBehaviour
	{
		public static StatsLog main;

		public Dictionary<int, Branch> branches = new Dictionary<int, Branch>();

		public List<AchievementId> achievements = new List<AchievementId>();

		private void Awake()
		{
			main = this;
		}

		public void ClearBranches()
		{
			List<int> list = new List<int>();
			foreach (Rocket rocket in GameManager.main.rockets)
			{
				list.Add(rocket.stats.branch);
			}
			foreach (Astronaut_EVA item in AstronautManager.main.eva)
			{
				list.Add(item.stats.branch);
			}
			ClearBranches(list);
		}

		private void ClearBranches(List<int> startBranches)
		{
			HashSet<int> traversed = new HashSet<int>();
			foreach (int startBranch in startBranches)
			{
				CollectParentBranches(startBranch);
			}
			int[] array = branches.Keys.ToArray();
			foreach (int num in array)
			{
				if (!traversed.Contains(num))
				{
					branches.Remove(num);
				}
			}
			void CollectParentBranches(int branch)
			{
				if (branches.ContainsKey(branch) && !traversed.Contains(branch))
				{
					traversed.Add(branch);
					Branch branch2 = branches[branch];
					if (branch2.parentA != -1)
					{
						CollectParentBranches(branch2.parentA);
					}
					if (branch2.parentB != -1)
					{
						CollectParentBranches(branch2.parentB);
					}
				}
			}
		}

		public void CreateRoot(out int newBranch)
		{
			CreateBranch(-1, -1, out newBranch);
		}

		public void SplitBranch(int branch, out int newBranch_A, out int newBranch_B)
		{
			CreateBranch(branch, -1, out newBranch_A);
			CreateBranch(branch, -1, out newBranch_B);
		}

		public void MergeBranch(int branch_A, int branch_B, out int newBranch)
		{
			CreateBranch(branch_A, branch_B, out newBranch);
		}

		private void CreateBranch(int parent_A, int parent_B, out int newBranch)
		{
			Branch value = new Branch
			{
				parentA = parent_A,
				parentB = parent_B,
				startTime = WorldTime.main.worldTime
			};
			newBranch = ((branches.Count != 0) ? (branches.Keys.Max() + 1) : 0);
			branches[newBranch] = value;
		}

		public void LoadState(int branch, out List<string> landed, out List<string> orbit, out List<string> orbit_Old, out List<string> atmosphere, out List<string> reentry, out List<string> crash)
		{
			landed = GetLastEvent(StatsRecorder.EventType.Landed, branch);
			orbit = GetLastEvent(StatsRecorder.EventType.Orbit, branch);
			orbit_Old = GetLastEvent(StatsRecorder.EventType.Orbit, branch, skipOne: true);
			atmosphere = GetLastEvent(StatsRecorder.EventType.Atmosphere, branch);
			reentry = GetLastEvent(StatsRecorder.EventType.Reentry, branch);
			crash = GetLastEvent(StatsRecorder.EventType.Crash, branch);
		}

		private List<string> GetLastEvent(StatsRecorder.EventType eventType, int branch, bool skipOne = false)
		{
			string eventName = eventType.ToString();
			return SearchBranch(branch);
			List<string> SearchBranch(int b)
			{
				if (!branches.ContainsKey(b))
				{
					return null;
				}
				Branch branch2 = branches[b];
				for (int num = branch2.events.Count - 1; num >= 0; num--)
				{
					if (branch2.events[num][0] == eventName)
					{
						if (!skipOne)
						{
							return branch2.events[num];
						}
						skipOne = false;
					}
				}
				return SearchBranch(branch2.parentA);
			}
		}
	}
}
