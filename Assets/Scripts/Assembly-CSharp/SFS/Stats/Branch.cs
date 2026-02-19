using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SFS.Parsers.Json;

namespace SFS.Stats
{
	[Serializable]
	public class Branch
	{
		[JsonProperty("a")]
		[LegacyName("parentA")]
		public int parentA = -1;

		[JsonProperty("b")]
		[LegacyName("parentB")]
		public int parentB = -1;

		[JsonProperty("T")]
		[LegacyName("startTime")]
		public double startTime;

		[JsonProperty("E")]
		[LegacyName("events")]
		public List<List<string>> events = new List<List<string>>();

		public void AddEvent(List<string> e)
		{
			events.Add(e);
		}
	}
}
