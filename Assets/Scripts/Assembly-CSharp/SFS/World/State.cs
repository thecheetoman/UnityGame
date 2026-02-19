using System;
using System.Collections.Generic;
using System.Globalization;
using SFS.Achievements;
using SFS.Stats;
using SFS.WorldBase;

namespace SFS.World
{
	public class State
	{
		private double startTime;

		public List<(string, double, AchievementId)> logs;

		public bool landed = true;

		private Planet planet = Base.planetLoader.spaceCenter.Planet;

		private StatsRecorder.Orbit_Tracker.State orbit = StatsRecorder.Orbit_Tracker.GetStartState();

		private StatsRecorder.Orbit_Tracker.State orbit_Old = StatsRecorder.Orbit_Tracker.GetStartState();

		private StatsRecorder.Atmosphere_Tracker.State atmosphere = StatsRecorder.Atmosphere_Tracker.GetStartState(new Location(Base.planetLoader.spaceCenter.Planet, Base.planetLoader.spaceCenter.LaunchPadLocation.position));

		public State(List<(string, double, AchievementId)> logs, double startTime)
		{
			this.logs = logs;
			this.startTime = startTime;
		}

		public static State Merge(State a, State b)
		{
			if (a.startTime > b.startTime)
			{
				State state = b;
				State state2 = a;
				a = state;
				b = state2;
			}
			a.logs.AddRange(b.logs);
			return a;
		}

		public State CopyState(double launchTime)
		{
			return new State(new List<(string, double, AchievementId)>(), launchTime)
			{
				planet = planet,
				landed = landed,
				orbit = orbit,
				orbit_Old = orbit_Old,
				atmosphere = atmosphere
			};
		}

		public void Log(AchievementId id, double reward, string msg)
		{
			if (msg != null)
			{
				logs.Add((msg, reward, id));
			}
		}

		public void LogAchievements(int branchId, ref bool space, ref bool LEO, ref bool moonLand)
		{
			foreach (List<string> @event in StatsLog.main.branches[branchId].events)
			{
				if (!Enum.TryParse<StatsRecorder.EventType>(@event[0], out var result))
				{
					continue;
				}
				switch (result)
				{
				case StatsRecorder.EventType.Planet:
				{
					Planet planet = @event[2].GetPlanet();
					AchievementsModule.Log_Planet(Log, planet, this.planet);
					landed = false;
					StatsRecorder.Orbit_Tracker.OnPlanetChange(out orbit, out orbit_Old);
					atmosphere = StatsRecorder.Atmosphere_Tracker.State.Outside;
					this.planet = planet;
					break;
				}
				case StatsRecorder.EventType.Landed:
				{
					landed = bool.Parse(@event[2]);
					double result2;
					bool flag = double.TryParse((@event.Count > 3) ? @event[3] : null, NumberStyles.Float, CultureInfo.InvariantCulture, out result2);
					AchievementsModule.Log_Landed(Log, landed, this.planet, flag ? result2 : double.NaN, endMissionMenu: true);
					if (landed && this.planet.codeName == "Moon")
					{
						moonLand = true;
					}
					break;
				}
				case StatsRecorder.EventType.Orbit:
				{
					StatsRecorder.Orbit_Tracker.State state2 = (StatsRecorder.Orbit_Tracker.State)Enum.Parse(typeof(StatsRecorder.Orbit_Tracker.State), @event[2]);
					AchievementsModule.Log_Orbit(Log, this.planet, state2, orbit, orbit_Old);
					orbit_Old = orbit;
					orbit = state2;
					if (orbit == StatsRecorder.Orbit_Tracker.State.Low && this.planet.codeName == "Earth")
					{
						LEO = true;
					}
					break;
				}
				case StatsRecorder.EventType.Atmosphere:
				{
					StatsRecorder.Atmosphere_Tracker.State state = (StatsRecorder.Atmosphere_Tracker.State)Enum.Parse(typeof(StatsRecorder.Atmosphere_Tracker.State), @event[2]);
					AchievementsModule.Log_Atmosphere(Log, this.planet, state, atmosphere, this.planet.codeName == Base.planetLoader.spaceCenter.address);
					atmosphere = state;
					if (state == StatsRecorder.Atmosphere_Tracker.State.Outside && this.planet.codeName == "Earth")
					{
						space = true;
					}
					break;
				}
				case StatsRecorder.EventType.Height:
					AchievementsModule.Log_ReachedHeight(Log, double.Parse(@event[2], CultureInfo.InvariantCulture));
					break;
				case StatsRecorder.EventType.Reentry:
					AchievementsModule.Log_Reentry(Log, this.planet, float.Parse(@event[2], CultureInfo.InvariantCulture));
					break;
				case StatsRecorder.EventType.LeftCapsule:
					AchievementsModule.Log_LeftCapsule(Log, this.planet, landed, orbit);
					break;
				case StatsRecorder.EventType.Flag:
					AchievementsModule.Log_Flag(Log, this.planet, double.Parse(@event[2]));
					break;
				case StatsRecorder.EventType.CollectRock:
					AchievementsModule.Log_CollectRock(Log, this.planet, double.Parse(@event[2]));
					break;
				case StatsRecorder.EventType.Crash:
					AchievementsModule.Log_Crash(Log, this.planet);
					break;
				}
			}
		}

		public void Log_Dock()
		{
			AchievementsModule.Log_Dock(Log, planet, landed, orbit);
		}
	}
}
