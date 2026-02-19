using SFS.Builds;
using SFS.IO;
using SFS.World;
using UnityEngine;

public class PlaytimeTracker : MonoBehaviour
{
	private int sessionPlaytime;

	private void Update()
	{
		if (Time.unscaledTime > (float)sessionPlaytime)
		{
			sessionPlaytime += 10;
			IncreaseSaveBy10("Total_Playtime");
			if (BuildManager.main != null || GameManager.main != null)
			{
				IncreaseSaveBy10("Active_Playtime");
			}
		}
	}

	private void IncreaseSaveBy10(string fileName)
	{
		int num = GetPlaytimeSeconds(fileName) + 10;
		FileLocations.GetNotificationsPath(fileName).WriteText(num.ToString());
	}

	public static int GetTotalPlaytimeSeconds()
	{
		return GetPlaytimeSeconds("Total_Playtime");
	}

	public static int GetActivePlaytimeSeconds()
	{
		return GetPlaytimeSeconds("Active_Playtime");
	}

	private static int GetPlaytimeSeconds(string fileName)
	{
		FilePath notificationsPath = FileLocations.GetNotificationsPath(fileName);
		if (notificationsPath.FileExists() && int.TryParse(notificationsPath.ReadText(), out var result))
		{
			return result;
		}
		return 0;
	}
}
