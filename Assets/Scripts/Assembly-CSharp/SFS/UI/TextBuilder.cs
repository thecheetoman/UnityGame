using System;

namespace SFS.UI
{
	public class TextBuilder : NewElementBuilder<TextBuilder>
	{
		private Func<string> text;

		private Action<TextAdapter> customize;

		private TextBuilder(NewElement prefab)
			: base(prefab)
		{
		}

		public TextBuilder Text(Func<string> text)
		{
			this.text = text;
			return this;
		}

		public TextBuilder Customize(Action<TextAdapter> action)
		{
			customize = (Action<TextAdapter>)Delegate.Combine(customize, action);
			return this;
		}

		public static TextBuilder CreateText()
		{
			return new TextBuilder(ElementGenerator.main.defaultText);
		}

		protected override void OnInstantiated(NewElement element)
		{
			TextAdapter componentInChildren = element.GetComponentInChildren<TextAdapter>();
			componentInChildren.Text = text();
			customize?.Invoke(componentInChildren);
		}
	}
}
