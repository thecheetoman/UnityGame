using UnityEngine;

namespace SFS.UI
{
	public class MsgDrawer : MonoBehaviour, I_MsgLogger
	{
		public static MsgDrawer main;

		public TextAdapter text;

		public AnimationClip msgAnimation;

		private float startTime = -10f;

		private void Awake()
		{
			main = this;
		}

		public void Log(string msg)
		{
			Log(msg, show: true);
		}

		public void Log(string msg, bool show)
		{
			if (show)
			{
				text.Text = msg;
				startTime = Time.unscaledTime;
				base.enabled = true;
				text.gameObject.SetActive(value: true);
			}
		}

		private void Update()
		{
			float num = Time.unscaledTime - startTime;
			if (num < msgAnimation.length)
			{
				msgAnimation.SampleAnimation(text.gameObject, num);
				return;
			}
			text.gameObject.SetActive(value: false);
			base.enabled = false;
		}
	}
}
