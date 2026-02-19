using UnityEngine;

namespace SFS.World.Drag
{
	public abstract class HeatModuleBase : MonoBehaviour
	{
		public bool disabled = true;

		public abstract string Name { get; }

		public abstract bool IsHeatShield { get; }

		public abstract float Temperature { get; set; }

		public abstract int LastAppliedIndex { get; set; }

		public abstract float ExposedSurface { get; set; }

		public abstract float HeatTolerance { get; }

		private void OnEnable()
		{
			disabled = false;
		}

		private void OnDisable()
		{
			disabled = true;
		}

		public abstract void OnOverheat(bool breakup);
	}
}
