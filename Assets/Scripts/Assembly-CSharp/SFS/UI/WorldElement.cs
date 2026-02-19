using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using SFS.IO;
using SFS.Translations;
using SFS.WorldBase;
using UnityEngine;

namespace SFS.UI
{
	public class WorldElement : MonoBehaviour
	{
		public TextAdapter worldNameText;

		public TextAdapter lastPlayedText;

		public TextAdapter timePlayedText;

		public TextAdapter difficultyText;

		public TextAdapter modeText;

		public ReorderingModule reorderingModule;

		public RadioButton radioButton;

		public Button button;

		public GameObject buttonHolder;

		public Button resumeGameButton;

		public Button renameButton;

		public WorldReference World { get; private set; }

		public string Name => World.worldName;

		public void Setup(WorldReference world, bool showMode)
		{
			World = world;
			worldNameText.Text = World.worldName;
			WorldSettings worldSettings = world.LoadWorldSettings();
			lastPlayedText.Text = (DateTime.Now - new DateTime((worldSettings.playtime.lastPlayedTime_Ticks == 0L) ? DateTime.Now.Ticks : worldSettings.playtime.lastPlayedTime_Ticks)).ToLastPlayedString();
			timePlayedText.Text = new TimeSpan(0, 0, 0, (int)worldSettings.playtime.totalPlayTime_Seconds).ToTimePlayedString();
			if (DevSettings.HasDifficulty)
			{
				difficultyText.Text = Loc.main.World_Difficulty_Name.Inject(worldSettings.difficulty.GetName(), "value");
			}
			else
			{
				difficultyText.gameObject.SetActive(value: false);
			}
			if (showMode)
			{
				modeText.Text = Loc.main.World_Mode_Name.Inject(worldSettings.mode.GetModeName(), "value");
			}
			else
			{
				modeText.gameObject.SetActive(value: false);
			}
		}

		private void CopyWorldData()
		{
			using MemoryStream memoryStream = new MemoryStream();
			GZipStream zipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true);
			try
			{
				Write(BitConverter.GetBytes(World.worldName.Length));
				Write(Encoding.UTF8.GetBytes(World.worldName));
				foreach (FilePath item in World.path.GetFilesInFolder(recursively: true))
				{
					WriteFileEntry(item, item.ReadBytes());
				}
				zipStream.Flush();
				zipStream.Close();
				GUIUtility.systemCopyBuffer = Convert.ToBase64String(memoryStream.ToArray());
			}
			finally
			{
				if (zipStream != null)
				{
					((IDisposable)zipStream).Dispose();
				}
			}
			void Write(byte[] data)
			{
				zipStream.Write(data, 0, data.Length);
			}
			void WriteFileEntry(string path, byte[] fileData)
			{
				path = path.Replace(string.Concat(World.path, "/"), "");
				Write(BitConverter.GetBytes(path.Length));
				Write(Encoding.UTF8.GetBytes(path));
				Write(BitConverter.GetBytes(fileData.Length));
				Write(fileData);
			}
		}
	}
}
