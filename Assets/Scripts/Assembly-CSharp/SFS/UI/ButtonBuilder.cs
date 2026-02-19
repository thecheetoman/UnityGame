using System;
using SFS.Input;

namespace SFS.UI
{
	public class ButtonBuilder : NewElementBuilder<ButtonBuilder>
	{
		private Func<string> text;

		private Action<Button> customizeButton;

		private Action onClick;

		private CloseMode closeMode;

		private ButtonBuilder(NewElement prefab)
			: base(prefab)
		{
		}

		public ButtonBuilder CustomizeButton(Action<Button> action)
		{
			customizeButton = action;
			return this;
		}

		public static ButtonBuilder CreateButton(SizeSyncerBuilder.Carrier sizeSync, Func<string> text, Action onClick, CloseMode closeMode)
		{
			ButtonBuilder buttonBuilder = new ButtonBuilder(ElementGenerator.main.defaultButton);
			if (sizeSync != null)
			{
				buttonBuilder.SizeSync(sizeSync);
			}
			buttonBuilder.MinSize(null, 60f);
			buttonBuilder.MaxSize(350f, null);
			buttonBuilder.Padding(40f, 10f);
			buttonBuilder.onClick = onClick;
			buttonBuilder.closeMode = closeMode;
			buttonBuilder.text = text;
			return buttonBuilder;
		}

		protected override void OnInstantiated(NewElement element)
		{
			Button component = element.GetComponent<Button>();
			switch (closeMode)
			{
			case CloseMode.Current:
				component.onClick += new Action(ScreenManager.main.CloseCurrent);
				break;
			case CloseMode.Stack:
				component.onClick += new Action(ScreenManager.main.CloseStack);
				break;
			}
			component.onClick += onClick;
			element.GetComponentInChildren<TextAdapter>().Text = text();
			customizeButton?.Invoke(component);
		}
	}
}
