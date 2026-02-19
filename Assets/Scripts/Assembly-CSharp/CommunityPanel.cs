using UnityEngine;

public class CommunityPanel : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	public void OpenPath()
	{
	}

	public void OpenYoutube()
	{
		Application.OpenURL("https://www.youtube.com/channel/UCOpgvpnGyZw4IRT_kuebiWA");
	}

	public void OpenDiscord()
	{
		Application.OpenURL("https://discordapp.com/invite/hwfWm2d");
	}

	public void OpenReddit()
	{
		Application.OpenURL("https://www.reddit.com/r/SpaceflightSimulator/");
	}
}
