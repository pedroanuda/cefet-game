using Godot;
using System;

namespace Game.UI
{
    public partial class MathGameOverScreen : PanelContainer
    {
        private Label SubtitleLabel;
        private AnimationPlayer _animPlayer;

        public void TriggerUi(string subtitleText = null)
        {
            GetTree().Paused = true;
            Show();
            SubtitleLabel.Text = subtitleText ?? "Incrivel!";
            _animPlayer.Play("appear");
        }

        private void OnSchoolButtonPressed()
        {
            GetTree().Paused = false;
            GetNode<Global>("/root/Global").TransitionToScene(
                "res://scenes/Scenarios/ClassroomScenario.tscn", 
                "Boa sorte na proxima vez!",
                1.5f
            );
        }

        public override void _Ready()
        {
            SubtitleLabel = GetNode<Label>("%GameOverSubtitle");
            _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        }
    }
}
