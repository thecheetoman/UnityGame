using System;
using SFS.World;

namespace SFS.WorldBase
{
	[Serializable]
	public class WorldSettings
	{
		public SolarSystemReference solarSystem;

		public Difficulty difficulty;

		public WorldMode mode;

		public WorldPlaytime playtime;

		public SandboxSettings.Data cheats;

		public WorldSettings()
		{
		}

		public WorldSettings(SolarSystemReference solarSystem, Difficulty difficulty, WorldMode mode, WorldPlaytime playtime, SandboxSettings.Data cheats)
		{
			this.solarSystem = solarSystem;
			this.difficulty = difficulty;
			this.mode = mode;
			this.playtime = playtime;
			this.cheats = cheats;
		}
	}
}
