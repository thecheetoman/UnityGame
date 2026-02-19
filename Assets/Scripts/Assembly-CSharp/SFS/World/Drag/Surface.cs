namespace SFS.World.Drag
{
	public struct Surface
	{
		public HeatModuleBase owner;

		public Line2 line;

		public Surface(HeatModuleBase owner, Line2 line)
		{
			this.owner = owner;
			this.line = line;
		}
	}
}
