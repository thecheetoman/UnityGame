using System;
using UnityEngine;

namespace SFS.Audio
{
	[Serializable]
	public class MusicTrack
	{
		public enum OnTrackEnd
		{
			PlayNext = 0,
			PlayRandom = 1
		}

		public string clipName;

		[Range(0f, 1f)]
		public float volume = 1f;

		[Range(-3f, 3f)]
		public float pitch = 1f;

		public OnTrackEnd onTrackEnd;
	}
}
