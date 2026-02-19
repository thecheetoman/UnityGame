using System;
using System.Collections.Generic;
using System.Linq;
using SFS.Stats;
using SFS.Translations;
using SFS.World;
using SFS.World.Maps;
using SFS.WorldBase;

namespace SFS.Achievements
{
	[Serializable]
	public class AchievementsModule
	{
		public bool Landed = true;

		public bool Takeoff = true;

		public bool Atmosphere = true;

		public bool Orbit = true;

		public bool Crash = true;

		public static void Log_Dock(Action<AchievementId, double, string> logger, Planet planet, bool landed, StatsRecorder.Orbit_Tracker.State orbit)
		{
			if (landed)
			{
				double arg = ((planet.codeName == Base.planetLoader.spaceCenter.address) ? 0.0 : (15.0 * planet.RewardMultiplier));
				logger(new AchievementId(AchievementType.Dock, 0, planet.codeName), arg, Loc.main.Docked_Surface.InjectField(planet.DisplayName, "planet"));
				return;
			}
			Field field = orbit switch
			{
				StatsRecorder.Orbit_Tracker.State.Low => Loc.main.Docked_Orbit_Low, 
				StatsRecorder.Orbit_Tracker.State.High => Loc.main.Docked_Orbit_High, 
				StatsRecorder.Orbit_Tracker.State.Esc => Loc.main.Docked_Escape, 
				StatsRecorder.Orbit_Tracker.State.Trans => Loc.main.Docked_Orbit_Transfer, 
				_ => Loc.main.Docked_Suborbital, 
			};
			logger(new AchievementId(AchievementType.Dock, 1, planet.codeName), 10.0 * planet.RewardMultiplier, field.InjectField(planet.DisplayName, "planet"));
		}

		public static void Log_Orbit(Action<AchievementId, double, string> logger, Planet planet, StatsRecorder.Orbit_Tracker.State orbit, StatsRecorder.Orbit_Tracker.State orbit_Old, StatsRecorder.Orbit_Tracker.State orbit_Old_Old)
		{
			if (!planet.data.achievements.Orbit)
			{
				return;
			}
			switch (orbit)
			{
			case StatsRecorder.Orbit_Tracker.State.Low:
				switch (orbit_Old)
				{
				case StatsRecorder.Orbit_Tracker.State.Sub:
				{
					bool flag = planet == Base.planetLoader.spaceCenter.Planet;
					Log(0, Loc.main.Reached_Low_Orbit, flag ? 60 : 20);
					break;
				}
				case StatsRecorder.Orbit_Tracker.State.Trans:
					if (orbit_Old_Old == StatsRecorder.Orbit_Tracker.State.Esc)
					{
						Log(1, Loc.main.Capture_Low_Orbit, 20.0);
					}
					else
					{
						Log(1, Loc.main.Descend_Low_Orbit, 20.0);
					}
					break;
				}
				break;
			case StatsRecorder.Orbit_Tracker.State.High:
				switch (orbit_Old)
				{
				case StatsRecorder.Orbit_Tracker.State.Trans:
					Log(3, Loc.main.Reached_High_Orbit, 25.0);
					break;
				case StatsRecorder.Orbit_Tracker.State.Esc:
					Log(3, Loc.main.Capture_High_Orbit, 15.0);
					break;
				}
				break;
			}
			void Log(int valueId, Field text, double reward)
			{
				logger(new AchievementId(AchievementType.Orbit, valueId, planet.codeName), reward * planet.RewardMultiplier, text.InjectField(planet.DisplayName, "planet"));
			}
		}

		public static void Log_Landed(Action<AchievementId, double, string> logger, bool landed, Planet planet, double angleDegrees, bool endMissionMenu)
		{
			if (!landed || !planet.data.achievements.Landed)
			{
				return;
			}
			Landmark landmark = null;
			double num = -1000.0;
			if (!double.IsNaN(angleDegrees))
			{
				Landmark[] landmarks = planet.landmarks;
				foreach (Landmark landmark2 in landmarks)
				{
					double num2 = (0.0 - Math.Abs(Math_Utility.NormalizeAngleDegrees((double)landmark2.data.Center - angleDegrees))) / ((double)landmark2.data.AngularWidth * 0.5);
					if (num2 > -1.1 && num2 > num)
					{
						landmark = landmark2;
						num = num2;
					}
				}
			}
			AchievementId arg = new AchievementId(AchievementType.Landed, 0, planet.codeName);
			double arg2 = 50.0 * planet.RewardMultiplier;
			if (landmark != null)
			{
				logger(arg, arg2, (endMissionMenu ? Loc.main.Landed_At_Landmark__Short : Loc.main.Landed_At_Landmark).InjectField(planet.DisplayName, "planet").InjectField(landmark.displayName, "landmark"));
			}
			else
			{
				logger(arg, arg2, Loc.main.Landed.InjectField(planet.DisplayName, "planet"));
			}
		}

		public static void Log_Planet(Action<AchievementId, double, string> logger, Planet planet, Planet planet_Old)
		{
			if (planet.parentBody == planet_Old)
			{
				logger(new AchievementId(AchievementType.Changed_SOI, 0, planet.codeName), 10.0, Loc.main.Entered_SOI.InjectField(planet.DisplayName, "planet"));
			}
			else
			{
				logger(new AchievementId(AchievementType.Changed_SOI, 1, planet_Old.codeName), 10.0, Loc.main.Escaped_SOI.InjectField(planet_Old.DisplayName, "planet"));
			}
		}

		public static void Log_Crash(Action<AchievementId, double, string> logger, Planet planet)
		{
			if (planet.data.achievements.Crash)
			{
				logger(new AchievementId(AchievementType.Crash, 0, planet.codeName), 0.0, Loc.main.Crashed_Into_Terrain.InjectField(planet.DisplayName, "planet"));
			}
		}

		public static void Log_LeftCapsule(Action<AchievementId, double, string> logger, Planet planet, bool landed, StatsRecorder.Orbit_Tracker.State orbit)
		{
			bool flag = planet == Base.planetLoader.spaceCenter.Planet;
			if (landed)
			{
				double arg = ((planet.codeName == Base.planetLoader.spaceCenter.address) ? 0.0 : (10.0 * planet.RewardMultiplier));
				logger(new AchievementId(AchievementType.LeftCapsule, 0, planet.codeName), arg, flag ? null : ((string)Loc.main.EVA_Surface.InjectField(planet.DisplayName, "planet")));
				return;
			}
			(int, Field, int) tuple = orbit switch
			{
				StatsRecorder.Orbit_Tracker.State.Low => (1, Loc.main.EVA_Orbit_Low, 0), 
				StatsRecorder.Orbit_Tracker.State.High => (2, Loc.main.EVA_Orbit_High, 10), 
				StatsRecorder.Orbit_Tracker.State.Esc => (3, Loc.main.EVA_Escape, 0), 
				StatsRecorder.Orbit_Tracker.State.Trans => (4, Loc.main.EVA_Orbit_Transfer, 0), 
				_ => (5, flag ? null : Loc.main.EVA_Suborbital, 0), 
			};
			(int, Field, double) tuple2 = (tuple.Item1, tuple.Item2, tuple.Item3);
			logger(new AchievementId(AchievementType.LeftCapsule, tuple2.Item1, planet.codeName), tuple2.Item3 * planet.RewardMultiplier, tuple2.Item2?.InjectField(planet.DisplayName, "planet"));
		}

		public static void Log_Flag(Action<AchievementId, double, string> logger, Planet planet, double angleDegrees)
		{
			double arg = ((planet.codeName == Base.planetLoader.spaceCenter.address) ? 0.0 : (10.0 * planet.RewardMultiplier));
			logger(new AchievementId(AchievementType.Flag, 0, planet.codeName), arg, Loc.main.Planted_Flag.InjectField(planet.DisplayName, "planet"));
		}

		public static void Log_CollectRock(Action<AchievementId, double, string> logger, Planet planet, double angleDegrees)
		{
			double arg = ((planet.codeName == Base.planetLoader.spaceCenter.address) ? 0.0 : (10.0 * planet.RewardMultiplier));
			logger(new AchievementId(AchievementType.CollectedRock, 0, planet.codeName), arg, Loc.main.Collected_Rock.InjectField(planet.DisplayName, "planet"));
		}

		public static void Log_Atmosphere(Action<AchievementId, double, string> logger, Planet planet, StatsRecorder.Atmosphere_Tracker.State state, StatsRecorder.Atmosphere_Tracker.State state_Old, bool hideEnteredAtmosphere)
		{
			if (!planet.data.achievements.Atmosphere)
			{
				return;
			}
			if (state == StatsRecorder.Atmosphere_Tracker.State.Lower && state_Old == StatsRecorder.Atmosphere_Tracker.State.Upper)
			{
				if (!hideEnteredAtmosphere)
				{
					logger(new AchievementId(AchievementType.Atmosphere, 0, planet.codeName), 0.0, Loc.main.Entered_Lower_Atmosphere.InjectField(planet.DisplayName, "planet"));
				}
				return;
			}
			switch (state)
			{
			case StatsRecorder.Atmosphere_Tracker.State.Upper:
				switch (state_Old)
				{
				case StatsRecorder.Atmosphere_Tracker.State.Outside:
					if (!hideEnteredAtmosphere)
					{
						logger(new AchievementId(AchievementType.Atmosphere, 1, planet.codeName), 0.0, Loc.main.Entered_Upper_Atmosphere.InjectField(planet.DisplayName, "planet"));
					}
					break;
				case StatsRecorder.Atmosphere_Tracker.State.Lower:
					if (planet != Base.planetLoader.spaceCenter.Planet)
					{
						logger(new AchievementId(AchievementType.Atmosphere, 2, planet.codeName), 0.0, Loc.main.Left_Lower_Atmosphere.InjectField(planet.DisplayName, "planet"));
					}
					break;
				}
				break;
			case StatsRecorder.Atmosphere_Tracker.State.Outside:
				if (state_Old == StatsRecorder.Atmosphere_Tracker.State.Upper)
				{
					bool flag = planet == Base.planetLoader.spaceCenter.Planet;
					string arg = (flag ? ((string)Loc.main.Reached_Karman_Line) : ((string)Loc.main.Left_Upper_Atmosphere.InjectField(planet.DisplayName, "planet")));
					logger(new AchievementId(AchievementType.Atmosphere, 3, planet.codeName), flag ? 50 : 15, arg);
				}
				break;
			}
		}

		public static void Log_End(Action<AchievementId, double, string> logger, Location location, bool landed)
		{
			Planet planet = location.planet;
			if (landed && planet.codeName == Base.planetLoader.spaceCenter.address)
			{
				logger(default(AchievementId), 0.0, Loc.main.Recover_Home.InjectField(planet.DisplayName, "planet"));
			}
		}

		public static void Log_ReachedHeight(Action<AchievementId, double, string> logger, double height)
		{
			int num = ((height == 15000.0) ? 20 : ((height == 10000.0) ? 15 : 10));
			logger(new AchievementId(AchievementType.Height, (int)height, null), num, Loc.main.Reached_Height.Inject((height > 2000.0) ? ((int)height).ToKmString() : height.ToDistanceString(decimals: false), "height"));
		}

		public static void Log_Reentry(Action<AchievementId, double, string> logger, Planet planet, float temperature)
		{
			logger(new AchievementId(AchievementType.Reentry, (int)temperature, planet.codeName), 0.0, Loc.main.Survived_Reentry.InjectField(planet.DisplayName, "planet").Inject(temperature.ToTemperatureString(), "temperature"));
		}

		public static (int countComplete, int countTotal) DrawAllAchievements(List<AchievementId> achievements, Action<string, Planet, List<string>> createElement)
		{
			List<Planet> planets = new List<Planet>();
			Add(Base.planetLoader.planets.Values.FirstOrDefault((Planet a) => a.parentBody == null));
			int num = 0;
			int num2 = 0;
			foreach (Planet item in planets)
			{
				if (item.parentBody == null)
				{
					continue;
				}
				List<string> text = new List<string>();
				int countComplete = 0;
				int countTotal = 0;
				bool flag = item == Base.planetLoader.spaceCenter.Planet;
				if (flag)
				{
					int[] altitudeMilestones = Base.worldBase.settings.difficulty.AltitudeMilestones;
					foreach (int num4 in altitudeMilestones)
					{
						Log_ReachedHeight(Log, num4);
					}
				}
				if (item.HasAtmospherePhysics && flag)
				{
					Log_Atmosphere(Log, item, StatsRecorder.Atmosphere_Tracker.State.Outside, StatsRecorder.Atmosphere_Tracker.State.Upper, hideEnteredAtmosphere: false);
				}
				if (flag)
				{
					Log_Orbit(Log, item, StatsRecorder.Orbit_Tracker.State.Low, StatsRecorder.Orbit_Tracker.State.Sub, StatsRecorder.Orbit_Tracker.State.None);
					Log_Orbit(Log, item, StatsRecorder.Orbit_Tracker.State.High, StatsRecorder.Orbit_Tracker.State.Trans, StatsRecorder.Orbit_Tracker.State.None);
					Log_LeftCapsule(Log, item, landed: false, StatsRecorder.Orbit_Tracker.State.Low);
				}
				else if (item.data.basics.gravity > 0.1)
				{
					Log_Orbit(Log, item, StatsRecorder.Orbit_Tracker.State.Low, StatsRecorder.Orbit_Tracker.State.Trans, StatsRecorder.Orbit_Tracker.State.Esc);
				}
				if (item.HasAtmospherePhysics && !flag)
				{
					Log_Atmosphere(Log, item, StatsRecorder.Atmosphere_Tracker.State.Upper, StatsRecorder.Atmosphere_Tracker.State.Outside, hideEnteredAtmosphere: false);
				}
				if (item.HasAtmospherePhysics)
				{
					float temperature = 0f;
					foreach (AchievementId achievement in achievements)
					{
						if (achievement.type == AchievementType.Reentry && achievement.planet == item.codeName)
						{
							temperature = achievement.value;
						}
					}
					Log_Reentry(Log, item, temperature);
				}
				if (!flag && item.data.hasTerrain && item.data.terrain.collider)
				{
					Log_Landed(Log, landed: true, item, double.NaN, endMissionMenu: true);
					if (!DevSettings.DisableAstronauts)
					{
						Log_LeftCapsule(Log, item, landed: true, StatsRecorder.Orbit_Tracker.State.None);
						Log_Flag(Log, item, double.NaN);
						if (item.data.terrain.rocks != null)
						{
							Log_CollectRock(Log, item, double.NaN);
						}
					}
				}
				if (text.Count > 0)
				{
					string arg = string.Concat(item.DisplayName, " ", countComplete.ToString(), "/", countTotal.ToString());
					createElement(arg, item, text);
					num += countComplete;
					num2 += countTotal;
				}
				void Log(AchievementId data, double _, string achievement)
				{
					if (achievement != null)
					{
						bool flag2 = achievements.Contains(data);
						text.Add((flag2 ? "<color=white>" : "<color=#ffffff80>") + " - " + achievement + "</color>");
						if (flag2)
						{
							countComplete++;
						}
						countTotal++;
					}
				}
			}
			return (countComplete: num, countTotal: num2);
			void Add(Planet a)
			{
				planets.Add(a);
				Planet[] satellites = a.satellites;
				foreach (Planet a2 in satellites)
				{
					Add(a2);
				}
			}
		}
	}
}
