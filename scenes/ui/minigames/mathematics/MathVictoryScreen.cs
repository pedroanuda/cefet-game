using Godot;
using System;

namespace Game.UI
{
    public partial class MathVictoryScreen : PanelContainer
    {
        public int Score { get; set; } = 0;
        public int DefeatedEnemies { get; set; } = 0;
        public int Time { get; set; } = 0;
        public int TotalScore { get; set; } = 0;

        private RichTextLabel _victoryLabel;
        private RichTextLabel _scoreLabel;
        private RichTextLabel _enemiesLabel;
        private RichTextLabel _timeLabel;
        private RichTextLabel _totalScoreLabel;
        private AnimationPlayer _animPlayer;
        private string _finishedAnimation = null;

        public void TriggerThing()
        {
            _scoreLabel.Text += Score.ToString();
            _enemiesLabel.Text += DefeatedEnemies.ToString();
            _timeLabel.Text += ConvertSecsToString(Time);
            _totalScoreLabel.Text += TotalScore.ToString();

            AdjustAnimation("score_label", "%ScoreLabel", _scoreLabel.Text.Length);
            AdjustAnimation("enemies_label", "%EnemiesLabel", _enemiesLabel.Text.Length);
            AdjustAnimation("time_label", "%TimeLabel", _timeLabel.Text.Length);
            AdjustAnimation("total_score_label", "%TotalScoreLabel", _totalScoreLabel.Text.Length);

            _animPlayer.Play("panel_appearing");
        }

        private void AdjustAnimation(StringName animName, string labelPath, int lenght)
        {
            var animation = _animPlayer.GetAnimation(animName);
            var idx = animation.FindTrack($"{labelPath}:visible_characters", Animation.TrackType.Value);
            animation.TrackSetKeyValue(idx, 1, lenght);
            animation.TrackSetKeyTime(idx, 1, lenght / 20f);
            animation.Length = lenght / 20f;
        }

        private void OnTimerOut()
        {
            if (_finishedAnimation is null) return;

            switch (_finishedAnimation)
            {
                case "panel_appearing":
                    _animPlayer.Play("win_label");
                    break;
                case "win_label":
                    _animPlayer.Play("score_label");
                    GetNode<Timer>("Timer").WaitTime = .8f;
                    break;
                case "score_label":
                    _animPlayer.Play("enemies_label");
                    break;
                case "enemies_label":
                    _animPlayer.Play("time_label");
                    break;
                case "time_label":
                    _animPlayer.Play("total_score_label");
                    break;
                case "total_score_label":
                    var schoolButton = GetNode<Button>("%SchoolButton");
                    schoolButton.Show();
                    _animPlayer.Play("show_button");
                    schoolButton.GrabFocus();
                    break;
            }
        }

        private void GoToSchool()
        {
            GetNode<Global>("/root/Global").TransitionToScene(
                "res://scenes/Scenarios/ClassroomScenario.tscn",
                "Parabens pela vitoria!",
                3
            );
        }

        public override void _Ready()
        {
            _victoryLabel = GetNode<RichTextLabel>("%VictoryLabel");
            _scoreLabel = GetNode<RichTextLabel>("%ScoreLabel");
            _enemiesLabel = GetNode<RichTextLabel>("%EnemiesLabel");
            _timeLabel = GetNode<RichTextLabel>("%TimeLabel");
            _totalScoreLabel = GetNode<RichTextLabel>("%TotalScoreLabel");
            _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _animPlayer.AnimationFinished += animName =>
            {
                _finishedAnimation = animName;
                GetNode<Timer>("Timer").Start();
            };
        }

        static string ConvertSecsToString(int time) =>
            $"{(time / 60).ToString().PadZeros(2)}:{(time % 60).ToString().PadZeros(2)}";
    }
}
