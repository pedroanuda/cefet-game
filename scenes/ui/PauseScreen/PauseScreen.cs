using Godot;
using System;

namespace Game.UI
{
    public partial class PauseScreen : Control
    {
        [Export]
        public bool MinigameLocation { get; set; } = false;

        private bool mouseWasVisible;

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

        private void OnResumeButtonPressed()
        {
            if (!mouseWasVisible) Input.MouseMode = Input.MouseModeEnum.Captured;

            GetTree().Paused = false;
            Hide();
        }

        private void GoToSchool()
        {
            OnResumeButtonPressed();
            GetNode<Global>("/root/Global")
                .TransitionToScene(
                "res://scenes/Scenarios/TesteScenario.tscn",
                "Explore o CEFET!",
                1.5f
                );
        }

        private void OnQuitButtonPressed() {
            GetTree().Quit();
        }

        public override void _Ready()
        {
            if (MinigameLocation)
            {
                var rows = GetNode("%OptionRows");
                var button = new Button()
                {
                    Text = "Voltar a escola",
                };
                button.Pressed += GoToSchool;
                rows.AddChild(button);
                rows.MoveChild(button, -2);
            }
        }
    }
}
