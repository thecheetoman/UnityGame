using SFS.World;
using UnityEngine;

namespace SFS.Parts.Modules
{
	public class SoftAttach : MonoBehaviour, Rocket.INJ_Rocket
	{
		public SurfaceData detach;

		public Rocket Rocket { private get; set; }
	}
}
