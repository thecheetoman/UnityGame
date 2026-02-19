using Beebyte.Obfuscator;
using SFS.Translations;

namespace SFS.WorldBase
{
	[Skip]
	public class WorldMode
	{
		[Skip]
		public enum Mode
		{
			Classic = 0,
			Career = 1,
			Sandbox = 2
		}

		public Mode mode;

		public bool AllowsCheats
		{
			get
			{
				if (mode != Mode.Classic)
				{
					return mode == Mode.Sandbox;
				}
				return true;
			}
		}

		public WorldMode(Mode mode)
		{
			this.mode = mode;
		}

		public string GetModeName()
		{
			return mode switch
			{
				Mode.Classic => Loc.main.Mode_Classic, 
				Mode.Career => Loc.main.Mode_Career, 
				Mode.Sandbox => Loc.main.Mode_Sandbox, 
				_ => "", 
			};
		}
	}
}
