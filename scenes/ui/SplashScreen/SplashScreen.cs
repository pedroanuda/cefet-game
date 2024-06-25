using Godot;
using System;
using System.Collections.Generic;

namespace Game
{
	public partial class SplashScreen : Control
	{
		[Export] private PackedScene _sceneToGoTo;
		private readonly List<SplashFrame> frames = new();
		private int pastFrames = 0;

		private void NextFrame()
		{
			if (pastFrames >= frames.Count)
			{
				GetTree().ChangeSceneToPacked(_sceneToGoTo);
				return;
			}

			frames[pastFrames].Play();
			frames[pastFrames].OnFinished += () => NextFrame();
			pastFrames++;
		}

		public override void _Ready()
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
			var godotFrames = FindChildren("*", "SplashFrame");
			foreach (Node frame in godotFrames)
			{
				frames.Add((SplashFrame)frame);
			}

			godotFrames.Clear();
			NextFrame();
		}
	}
}
