using System;
using System.Collections.Generic;
using System.Globalization;
using Beebyte.Obfuscator;
using SFS.Achievements;
using SFS.UI;
using SFS.World;
using SFS.World.Drag;
using SFS.WorldBase;
using UnityEngine;

namespace SFS.Stats
{
	public class StatsRecorder : MonoBehaviour
	{
		public abstract class Tracker
		{
			protected StatsRecorder owner;

			protected Tracker(StatsRecorder owner)
			{
				this.owner = owner;
			}
		}

		public class Landed_Tracker : Tracker
		{
			public bool state;

			public Landed_Tracker(bool state, StatsRecorder owner)
				: base(owner)
			{
				this.state = state;
			}

			public void Record(Location location)
			{
				bool flag = ((!state) ? location.velocity.Mag_LessThan(0.001) : (location.velocity.Mag_LessThan(2.0) || location.TerrainHeight < 300.0));
				if (flag != state)
				{
					state = flag;
					owner.RecordEvent(Log_Landed(flag, location.position.AngleDegrees));
					if ((bool)owner.player.isPlayer)
					{
						AchievementsModule.Log_Landed(ShowMsg, state, owner.location.planet.Value, location.position.AngleDegrees, endMissionMenu: false);
					}
				}
			}
		}

		public class Height_Tracker : Tracker
		{
			public double state;

			public Height_Tracker(double state, StatsRecorder owner)
				: base(owner)
			{
				this.state = state;
			}

			public void Record(Location location)
			{
				double height = location.Height;
				if (location.planet == Base.planetLoader.spaceCenter.Planet)
				{
					int[] altitudeMilestones = Base.worldBase.settings.difficulty.AltitudeMilestones;
					foreach (int num in altitudeMilestones)
					{
						if (state <= (double)num && height > (double)num)
						{
							owner.RecordEvent(Log_Height(num));
							if ((bool)owner.player.isPlayer)
							{
								AchievementsModule.Log_ReachedHeight(ShowMsg, num);
							}
						}
					}
				}
				state = height;
			}
		}

		public class Orbit_Tracker : Tracker
		{
			[Skip]
			public enum State
			{
				None = 0,
				Sub = 1,
				Low = 2,
				Trans = 3,
				High = 4,
				Esc = 5
			}

			public State state;

			public State state_Old;

			public Orbit_Tracker(State state, State state_Old, StatsRecorder owner)
				: base(owner)
			{
				this.state = state;
				this.state_Old = state_Old;
			}

			public void Record(Location location)
			{
				State state = GetState(location, owner, owner.landed_Tracker.state);
				if (state != this.state)
				{
					owner.RecordEvent(Log_Orbit(state));
					if ((bool)owner.player.isPlayer)
					{
						AchievementsModule.Log_Orbit(ShowMsg, owner.location.planet.Value, state, this.state, state_Old);
					}
					state_Old = this.state;
					this.state = state;
				}
			}

			public static State GetState(Location location, StatsRecorder owner, bool landed)
			{
				bool success;
				Orbit orbit = Orbit.TryCreateOrbit(location, calculateTimeParameters: false, calculateEncounters: false, out success);
				if (!success || landed)
				{
					return State.None;
				}
				if (orbit.apoapsis > location.planet.SOI)
				{
					return State.Esc;
				}
				if (orbit.periapsis < location.planet.OrbitRadius)
				{
					return State.Sub;
				}
				if (orbit.periapsis > location.planet.OrbitRadius * 2.5)
				{
					return State.High;
				}
				if (orbit.apoapsis > location.planet.OrbitRadius * 1.5)
				{
					return State.Trans;
				}
				return State.Low;
			}

			public static State GetStartState()
			{
				return State.Sub;
			}

			public static void OnPlanetChange(out State orbit, out State orbit_Old)
			{
				orbit = State.Sub;
				orbit_Old = State.Sub;
			}
		}

		public class Atmosphere_Tracker : Tracker
		{
			[Skip]
			public enum State
			{
				Lower = 0,
				Upper = 1,
				Outside = 2
			}

			public State state;

			public Atmosphere_Tracker(State state, StatsRecorder owner)
				: base(owner)
			{
				this.state = state;
			}

			public void Record(Location location)
			{
				State state = GetState(location);
				if (state != this.state)
				{
					owner.RecordEvent(Log_Atmosphere(state));
					if ((bool)owner.player.isPlayer)
					{
						AchievementsModule.Log_Atmosphere(ShowMsg, owner.location.planet.Value, state, this.state, hideEnteredAtmosphere: false);
					}
					this.state = state;
				}
			}

			public static State GetState(Location location)
			{
				if (!location.planet.HasAtmospherePhysics)
				{
					return State.Outside;
				}
				double height = location.Height;
				if (height > location.planet.AtmosphereHeightPhysics)
				{
					return State.Outside;
				}
				if (height > location.planet.AtmosphereHeightPhysics * location.planet.data.atmospherePhysics.upperAtmosphere)
				{
					return State.Upper;
				}
				return State.Lower;
			}

			public static State GetStartState(Location location)
			{
				return GetState(location);
			}
		}

		public class Reentry_Tracker : Tracker
		{
			public List<string> lastEvent;

			public Reentry_Tracker(List<string> lastEvent, StatsRecorder owner)
				: base(owner)
			{
				this.lastEvent = lastEvent;
			}

			public void Record(Location location, Player player)
			{
				float num = 0f;
				if (AeroModule.IsInsideAtmosphereAndIsMoving(location))
				{
					HeatModuleBase[] componentsInChildren = player.GetComponentsInChildren<HeatModuleBase>();
					foreach (HeatModuleBase heatModuleBase in componentsInChildren)
					{
						if (!float.IsInfinity(heatModuleBase.Temperature))
						{
							num = Mathf.Max(num, heatModuleBase.Temperature);
						}
					}
				}
				else
				{
					num = 0f;
				}
				if (num > 100f && location.VerticalVelocity < 0.0)
				{
					if (lastEvent == null)
					{
						lastEvent = Log_Reentry(num);
						owner.RecordEvent(lastEvent);
					}
					if (num > float.Parse(lastEvent[2], CultureInfo.InvariantCulture))
					{
						lastEvent[2] = num.ToString(CultureInfo.InvariantCulture);
					}
				}
				else if (num < 10f && lastEvent != null)
				{
					lastEvent = null;
				}
			}

			public void NewEvent()
			{
				lastEvent = null;
			}
		}

		public class Crash_Tracker : Tracker
		{
			private List<string> lastEvent;

			public Crash_Tracker(List<string> lastCrashEvent, StatsRecorder owner)
				: base(owner)
			{
				lastEvent = lastCrashEvent;
			}

			public void OnCrash()
			{
				if (lastEvent == null || WorldTime.main.worldTime - double.Parse(lastEvent[3]) > 10.0)
				{
					lastEvent = Log_Crash(1, WorldTime.main.worldTime);
					owner.RecordEvent(lastEvent);
				}
				else
				{
					lastEvent[2] = (int.Parse(lastEvent[2]) + 1).ToString();
					lastEvent[3] = WorldTime.main.worldTime.ToString(CultureInfo.InvariantCulture);
				}
				if ((bool)owner.player.isPlayer)
				{
					AchievementsModule.Log_Crash(ShowMsg, owner.location.planet.Value);
				}
			}

			public void NewEvent()
			{
				lastEvent = null;
			}
		}

		[Serializable]
		public enum EventType
		{
			Landed = 0,
			Height = 1,
			Orbit = 2,
			Atmosphere = 3,
			Reentry = 4,
			Planet = 5,
			LeftCapsule = 6,
			Flag = 7,
			CollectRock = 8,
			Crash = 9
		}

		private const float RecordTime = 1f;

		public WorldLocation location;

		public Player player;

		public int branch = -1;

		private Location location_Old;

		public Landed_Tracker landed_Tracker;

		public Height_Tracker height_Tracker;

		public Orbit_Tracker orbit_Tracker;

		public Atmosphere_Tracker atmosphere_Tracker;

		public Reentry_Tracker reentry_Tracker;

		public Crash_Tracker crash_Tracker;

		private static string Time => WorldTime.main.worldTime.Round(1).ToString();

		public void Load(int branch)
		{
			if (!StatsLog.main.branches.ContainsKey(branch))
			{
				StatsLog.main.CreateRoot(out branch);
				Location value = location.Value;
				bool landed = value.velocity.Mag_LessThan(1.0);
				Initialize(branch, landed, value.Height, Orbit_Tracker.GetState(value, this, landed), Orbit_Tracker.GetState(value, this, landed), Atmosphere_Tracker.GetState(value), null, null);
			}
			else
			{
				StatsLog.main.LoadState(branch, out var landed2, out var orbit, out var orbit_Old, out var atmosphere, out var reentry, out var crash);
				bool landed3 = landed2 != null && bool.Parse(landed2[2]);
				Orbit_Tracker.State orbit2 = ((orbit != null) ? ((Orbit_Tracker.State)Enum.Parse(typeof(Orbit_Tracker.State), orbit[2])) : Orbit_Tracker.GetStartState());
				Orbit_Tracker.State orbit_Old2 = ((orbit_Old != null) ? ((Orbit_Tracker.State)Enum.Parse(typeof(Orbit_Tracker.State), orbit_Old[2])) : Orbit_Tracker.GetStartState());
				Atmosphere_Tracker.State atmosphere2 = ((atmosphere != null) ? ((Atmosphere_Tracker.State)Enum.Parse(typeof(Atmosphere_Tracker.State), atmosphere[2])) : Atmosphere_Tracker.GetStartState(location.Value));
				Initialize(branch, landed3, location.Value.Height, orbit2, orbit_Old2, atmosphere2, reentry, crash);
			}
		}

		public static void OnSplit(StatsRecorder A, StatsRecorder B)
		{
			int oldBranch = A.branch;
			StatsLog.main.SplitBranch(oldBranch, out var branch_A, out var branch_B);
			A.branch = branch_A;
			var (lastEvent, reentry) = SplitReentry();
			A.reentry_Tracker.lastEvent = lastEvent;
			A.crash_Tracker.NewEvent();
			B.Initialize(branch_B, A.landed_Tracker.state, A.height_Tracker.state, A.orbit_Tracker.state, A.orbit_Tracker.state_Old, A.atmosphere_Tracker.state, reentry, null);
			(List<string> a, List<string> b) SplitReentry()
			{
				List<string> lastEvent2 = A.reentry_Tracker.lastEvent;
				if (lastEvent2 == null)
				{
					return (a: null, b: null);
				}
				StatsLog.main.branches[oldBranch].events.Remove(lastEvent2);
				float num = float.Parse(lastEvent2[2], CultureInfo.InvariantCulture);
				List<string> list = Log_Reentry(num);
				List<string> list2 = Log_Reentry(num);
				StatsLog.main.branches[branch_A].AddEvent(list);
				StatsLog.main.branches[branch_B].AddEvent(list2);
				return (a: list, b: list2);
			}
		}

		public static void OnMerge(StatsRecorder A, StatsRecorder B)
		{
			StatsLog.main.MergeBranch(A.branch, B.branch, out var newBranch);
			A.branch = newBranch;
			A.crash_Tracker.NewEvent();
			A.reentry_Tracker.NewEvent();
			if (B.landed_Tracker.state != A.landed_Tracker.state)
			{
				B.RecordEvent(Log_Landed(A.landed_Tracker.state, B.location.position.Value.AngleDegrees));
			}
			if (B.orbit_Tracker.state != A.orbit_Tracker.state)
			{
				B.RecordEvent(Log_Orbit(A.orbit_Tracker.state));
			}
			if (B.atmosphere_Tracker.state != A.atmosphere_Tracker.state)
			{
				B.RecordEvent(Log_Atmosphere(A.atmosphere_Tracker.state));
			}
			if ((bool)A.player.isPlayer)
			{
				AchievementsModule.Log_Dock(ShowMsg, A.location.planet, A.landed_Tracker.state, A.orbit_Tracker.state);
			}
		}

		private void OnPlanetChange(Planet oldPlanet, Planet newPlanet)
		{
			if (!(oldPlanet == null) && !(newPlanet == null) && !(oldPlanet == newPlanet))
			{
				RecordEvent(Log_Planet(newPlanet.codeName, location.position.Value.AngleDegrees, location.velocity.Value));
				if ((bool)player.isPlayer)
				{
					AchievementsModule.Log_Planet(ShowMsg, newPlanet, oldPlanet);
				}
				landed_Tracker.state = false;
				height_Tracker.state = double.PositiveInfinity;
				Orbit_Tracker.OnPlanetChange(out orbit_Tracker.state, out orbit_Tracker.state_Old);
				atmosphere_Tracker.state = Atmosphere_Tracker.State.Outside;
				reentry_Tracker.NewEvent();
				crash_Tracker.NewEvent();
			}
		}

		private void Initialize(int branch, bool landed, double height, Orbit_Tracker.State orbit, Orbit_Tracker.State orbit_Old, Atmosphere_Tracker.State atmosphere, List<string> reentry, List<string> lastCrashEvent)
		{
			this.branch = branch;
			landed_Tracker = new Landed_Tracker(landed, this);
			height_Tracker = new Height_Tracker(height, this);
			orbit_Tracker = new Orbit_Tracker(orbit, orbit_Old, this);
			atmosphere_Tracker = new Atmosphere_Tracker(atmosphere, this);
			reentry_Tracker = new Reentry_Tracker(reentry, this);
			crash_Tracker = new Crash_Tracker(lastCrashEvent, this);
		}

		private void Start()
		{
			location.planet.OnChange += new Action<Planet, Planet>(OnPlanetChange);
			InvokeRepeating("Record", UnityEngine.Random.Range(0.5f, 1.5f), 1f);
		}

		private void Record()
		{
			Location value = location.Value;
			landed_Tracker.Record(value);
			height_Tracker.Record(value);
			orbit_Tracker.Record(value);
			atmosphere_Tracker.Record(value);
			reentry_Tracker.Record(value, player);
		}

		public void OnLeaveCapsule(string astronautName)
		{
			RecordEvent(Log_LeftCapsule(astronautName));
		}

		public void OnPlantFlag(double angleDegrees)
		{
			RecordEvent(Log_Flag(angleDegrees));
			AchievementsModule.Log_Flag(ShowMsg, location.planet.Value, angleDegrees);
		}

		public void OnCollectRock(double angleDegrees)
		{
			RecordEvent(Log_CollectRock(angleDegrees));
			AchievementsModule.Log_CollectRock(ShowMsg, location.planet.Value, angleDegrees);
		}

		public void OnCrash()
		{
			crash_Tracker.OnCrash();
		}

		private void RecordEvent(List<string> data)
		{
			StatsLog.main.branches[branch].AddEvent(data);
		}

		private static void ShowMsg(AchievementId id, double reward, string msg)
		{
			MsgDrawer.main.Log(msg);
		}

		public bool HasFlown()
		{
			List<int> list = new List<int>();
			int num = branch;
			while (true)
			{
				if (list.Count > 1000)
				{
					MsgDrawer.main.Log("LOOP");
					return false;
				}
				list.Add(num);
				int parentA = StatsLog.main.branches[num].parentA;
				if (parentA == -1)
				{
					break;
				}
				num = parentA;
			}
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				foreach (List<string> @event in StatsLog.main.branches[list[num2]].events)
				{
					if (@event[0] == EventType.Landed.ToString() && @event[2] == false.ToString())
					{
						return true;
					}
				}
			}
			return false;
		}

		private static List<string> Log_Landed(bool landed, double angle)
		{
			return new List<string>
			{
				EventType.Landed.ToString(),
				Time,
				landed.ToString(),
				angle.ToString()
			};
		}

		private static List<string> Log_Height(int height)
		{
			return new List<string>
			{
				EventType.Height.ToString(),
				Time,
				height.ToString()
			};
		}

		private static List<string> Log_Orbit(Orbit_Tracker.State state)
		{
			return new List<string>
			{
				EventType.Orbit.ToString(),
				Time,
				state.ToString()
			};
		}

		private static List<string> Log_Atmosphere(Atmosphere_Tracker.State state)
		{
			return new List<string>
			{
				EventType.Atmosphere.ToString(),
				Time,
				state.ToString()
			};
		}

		private static List<string> Log_Reentry(double maxTemp)
		{
			return new List<string>
			{
				EventType.Reentry.ToString(),
				Time,
				maxTemp.ToString(CultureInfo.InvariantCulture)
			};
		}

		private static List<string> Log_Planet(string planet, double angle, Double2 velocity)
		{
			return new List<string>
			{
				EventType.Planet.ToString(),
				Time,
				planet,
				angle.ToString(),
				velocity.x.ToString(),
				velocity.y.ToString()
			};
		}

		private static List<string> Log_LeftCapsule(string astronautName)
		{
			return new List<string>
			{
				EventType.LeftCapsule.ToString(),
				Time,
				astronautName
			};
		}

		private static List<string> Log_Flag(double angle)
		{
			return new List<string>
			{
				EventType.Flag.ToString(),
				Time,
				angle.ToString()
			};
		}

		private static List<string> Log_CollectRock(double angle)
		{
			return new List<string>
			{
				EventType.CollectRock.ToString(),
				Time,
				angle.ToString()
			};
		}

		private static List<string> Log_Crash(int count, double lastCrashTime)
		{
			return new List<string>
			{
				EventType.Crash.ToString(),
				Time,
				count.ToString(),
				lastCrashTime.ToString()
			};
		}
	}
}
