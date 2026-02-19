using System;

namespace SFS.Achievements
{
	[Serializable]
	public struct AchievementId
	{
		public AchievementType type;

		public int value;

		public string planet;

		public AchievementId(AchievementType type, int value, string planet)
		{
			this.type = type;
			this.value = value;
			this.planet = planet;
		}
	}
}
