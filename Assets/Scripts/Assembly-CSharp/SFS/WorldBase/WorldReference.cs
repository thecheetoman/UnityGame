using System;
using System.Collections.Generic;
using SFS.IO;
using SFS.Parsers.Json;
using SFS.World;

namespace SFS.WorldBase
{
	[Serializable]
	public class WorldReference
	{
		public string worldName;

		public readonly FolderPath path;

		public readonly FilePath worldSettingsPath;

		public readonly FolderPath buildPersistentPath;

		public readonly FolderPath worldPersistentPath;

		public readonly FolderPath revertLaunchPath;

		public readonly FolderPath quicksavesPath;

		public WorldReference(string worldName)
		{
			this.worldName = worldName;
			path = FileLocations.WorldsFolder.Extend(worldName);
			worldSettingsPath = path.ExtendToFile("WorldSettings.txt");
			buildPersistentPath = path.CloneAndExtend("BlueprintPersistent");
			worldPersistentPath = path.CloneAndExtend("Persistent");
			revertLaunchPath = path.CloneAndExtend("RevertLaunch");
			quicksavesPath = path.CloneAndExtend("Quicksaves");
		}

		public bool WorldExists()
		{
			return FileLocations.WorldsFolder.Extend(worldName).FolderExists();
		}

		public WorldSettings LoadWorldSettings()
		{
			SolarSystemReference data2;
			WorldPlaytime data3;
			if (!JsonWrapper.TryLoadJson<WorldSettings>(worldSettingsPath, out var data) || data == null)
			{
				return new WorldSettings((JsonWrapper.TryLoadJson<SolarSystemReference>(path.ExtendToFile("Solar System.txt"), out data2) && data2 != null) ? data2 : new SolarSystemReference(""), playtime: (JsonWrapper.TryLoadJson<WorldPlaytime>(path.ExtendToFile("Playtime.txt"), out data3) && data3 != null) ? data3 : new WorldPlaytime(DateTime.Now.Ticks, 0.0), difficulty: Difficulty.Normal, mode: new WorldMode(WorldMode.Mode.Classic), cheats: new SandboxSettings.Data());
			}
			if (data.difficulty == null)
			{
				data.difficulty = Difficulty.Normal;
			}
			data.mode = new WorldMode(WorldMode.Mode.Classic);
			return data;
		}

		public void SaveWorldSettings(WorldSettings settings)
		{
			Difficulty difficulty = settings.difficulty;
			WorldMode mode = settings.mode;
			settings.mode = null;
			JsonWrapper.SaveAsJson(worldSettingsPath, settings, pretty: false);
			settings.difficulty = difficulty;
			settings.mode = mode;
		}

		public OrderedPathList GetQuicksavesFileList()
		{
			if (!quicksavesPath.FolderExists())
			{
				quicksavesPath.CreateFolder();
			}
			List<BasePath> list = new List<BasePath>(quicksavesPath.GetFoldersInFolder(recursively: false));
			return new OrderedPathList(quicksavesPath, list.ToArray());
		}

		public bool CanResumeGame()
		{
			return WorldSave.Load_WorldState(worldPersistentPath).playerAddress != "null";
		}

		public FolderPath GetQuicksavePath(string quicksaveName)
		{
			return quicksavesPath.CloneAndExtend(quicksaveName);
		}
	}
}
