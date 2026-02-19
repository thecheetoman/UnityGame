using System;
using System.Collections.Generic;
using System.Linq;
using SFS.Variables;
using SFS.World;
using SFS.World.Drag;
using UnityEngine;

namespace SFS.Parts.Modules
{
	public class WingModule : MonoBehaviour, Rocket.INJ_TurnAxisTorque, Rocket.INJ_Location, I_InitializePartModule
	{
		public SurfaceData wingSurface;

		public AnimationCurve liftAtSpeed;

		public AnimationCurve liftAtAngleOfAttack;

		public Composed_Vector2 flightDirection;

		public Composed_Vector2 centerOfLift;

		public Composed_Float areaMultiplier;

		public MoveModule elevator;

		private readonly Float_Local turnAxis = new Float_Local();

		private float? _wingArea;

		public Location Location { private get; set; }

		public float TurnAxis
		{
			set
			{
				turnAxis.Value = value;
			}
		}

		public int Priority => -1;

		public float WingArea
		{
			get
			{
				float? num = (_wingArea = _wingArea ?? CalculateWingArea());
				return num.GetValueOrDefault();
			}
		}

		public void Initialize()
		{
			turnAxis.OnChange += new Action(RecalculateElevatorDeflection);
		}

		private void RecalculateElevatorDeflection()
		{
			if (elevator != null && Location != null)
			{
				elevator.targetTime.Value = (AeroModule.IsInsideAtmosphereAndIsMoving(Location) ? (turnAxis.Value * base.transform.RotationDirection()) : 0f);
			}
		}

		public void RecalculateWingArea()
		{
			_wingArea = null;
		}

		private float CalculateWingArea()
		{
			List<Vector2> source = (from x in wingSurface.surfaces.SelectMany((Surfaces x) => x.points)
				orderby Vector2.Dot(x, flightDirection.Value)
				select x).ToList();
			return (source.Last() - source.First()).magnitude;
		}
	}
}
