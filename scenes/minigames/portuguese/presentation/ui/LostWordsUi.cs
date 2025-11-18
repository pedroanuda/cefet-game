using System;
using Game.Minigames.Portuguese.Domain;
using Game.UI;
using Godot;

namespace Game.Minigames.Portuguese.Presentation
{
    public partial class LostWordsUi : Control
    {
        private Label minigameNameLabel;
        private Label wordsLabel;
        private PauseScreen pauseScreen;
        private AnimationPlayer animationPlayer;

        private bool onEndScreen = false;

        public override void _Ready()
        {
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            minigameNameLabel = GetNode<Label>("%MinigameNameLabel");
            pauseScreen = GetParent().GetNode<PauseScreen>("PauseScreen");
            wordsLabel = GetNode<Label>("%WordsLabel");
            Show();
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionReleased("go_back") && !onEndScreen)
            {
                pauseScreen.HandlePause();
            }
        }

        public void EnterLevel(Level levelInfo)
        {
            minigameNameLabel.Text = levelInfo.Name;
            minigameNameLabel.Visible = true;

            void labelAnimation(StringName animName)
            {
                if (animName != "enter_level") return;
                animationPlayer.AnimationFinished -= labelAnimation;
                minigameNameLabel.Visible = false;
            }

            animationPlayer.AnimationFinished += labelAnimation;
            animationPlayer.Play("enter_level");
        }

        public void ExitLevel()
        {
            minigameNameLabel.Visible = false;
            wordsLabel.Visible = false;
        }

        public void ShowSentence(Sentence sentence)
        {
            wordsLabel.Text = sentence.GetMaskedSentence();

            wordsLabel.Visible = true;
            wordsLabel.Modulate = new Color(1, 1, 1, 0);

            Tween tween = CreateTween();
            tween.TweenProperty(wordsLabel, "modulate:a", 1, 0.5).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        }

        public void UpdateSentence(Sentence sentence)
        {
            wordsLabel.Text = sentence.GetMaskedSentence();
        }

        public void ShowEndScreen(GameStatistics statistics)
        {
            onEndScreen = true;

            GetNode<Label>("%ErrorsLabel").Text = $"Erros cometidos: {statistics.ErrorsMade}";
            GetNode<Label>("%HintsLabel").Text = $"Dicas utilizadas: {statistics.HintsUsed}";
            GetNode<Label>("%TotalScoreLabel").Text = $"Pontua\u00E7\u00E3o Total: {statistics.TotalScore}";

            void onAnimationFinished(StringName animName)
            {

                animationPlayer.AnimationFinished -= onAnimationFinished;
                GetNode<Button>("%BackToSchoolButton").GrabFocus();
            }

            animationPlayer.AnimationFinished += onAnimationFinished;
            animationPlayer.Play("show_end_screen");
        }

        private void OnBackToSchoolPressed()
        {
            GetNode<Global>("/root/Global")
            .TransitionToScene(
                "res://scenes/Scenarios/ClassroomScenario.tscn",
                "Explore o CEFET!",
                1.5f
            );
        }
    }
}