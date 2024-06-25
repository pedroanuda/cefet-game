using Godot;
using System;

namespace Game.UI
{
	public partial class MainMenu : Control
	{
		[Export]
		private PackedScene NewGameScene;

		public override void _Ready()
		{
			var button = GetNode<Button>("MarginContainer/CenterContainer/VBoxContainer/NewGameButton");
			button.GrabFocus();
		}

		private void OnNewGameButtonPressed()
		{
			GetTree().ChangeSceneToPacked(NewGameScene);
		}

		private void OnQuitButtonPressed()
		{
			GetTree().Quit();
		}
	}
}
