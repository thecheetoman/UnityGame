using System.Collections.Generic;
using System.Linq;
using SFS.Career;
using UnityEngine;

public class ResourcesLoader : MonoBehaviour
{
	public static ResourcesLoader main;

	public Dictionary<string, TT_PartPackData> partPacks;

	private void Awake()
	{
		main = this;
		partPacks = GetFiles_Dictionary<TT_PartPackData>("Career_PartPacks");
	}

	public static Dictionary<string, T> GetFiles_Dictionary<T>(string path) where T : Object
	{
		return GetFiles_Array<T>(path).ToDictionary((T x) => x.name, (T x) => x);
	}

	public static T[] GetFiles_Array<T>(string path) where T : Object
	{
		return Resources.LoadAll<T>(path);
	}
}
