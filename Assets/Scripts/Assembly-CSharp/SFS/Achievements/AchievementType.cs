using System;

namespace SFS.Achievements
{
	[Serializable]
	public enum AchievementType
	{
		Null = 0,
		Landed = 1,
		Dock = 2,
		Orbit = 3,
		Changed_SOI = 4,
		Crash = 5,
		Atmosphere = 6,
		Height = 7,
		Reentry = 8,
		LeftCapsule = 9,
		Flag = 10,
		CollectedRock = 11
	}
}
