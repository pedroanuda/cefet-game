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
			var button = GetNode<Button>("MarginContainer/VBoxContainer/NewGameButton");
			button.GrabFocus();
		}

		private void OnNewGameButtonPressed()
		{
			GetNode<Global>("/root/Global").TransitionToScene(NewGameScene.ResourcePath);
		}

		private void OnQuitButtonPressed()
		{
			GetTree().Quit();
		}
	}
}
