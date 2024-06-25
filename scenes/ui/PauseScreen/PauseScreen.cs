using Godot;
using System;

namespace Game.UI
{
    public partial class PauseScreen : Control
    {
        private bool mouseWasVisible;

        private void OnResumeButtonPressed()
        {
            if (!mouseWasVisible) Input.MouseMode = Input.MouseModeEnum.Captured;

            GetTree().Paused = false;
            Hide();
        }

        private void OnQuitButtonPressed() {
            GetTree().Quit();
        }

        public void HandlePause()
        {
            mouseWasVisible = Input.MouseMode == Input.MouseModeEnum.Visible;

            if (GetTree().Paused) OnResumeButtonPressed();
            else
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
                GetTree().Paused = true;
                Show();
            }
        }
    }
}
