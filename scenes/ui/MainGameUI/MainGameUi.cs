using Godot;
using System;

namespace Game.UI
{
    public partial class MainGameUi : Control
    {
        private ProgressBar xpProgressbar;
        private Label levelLabel;
        private Label patentLabel;

        public override void _Ready()
        {
            xpProgressbar = GetNode<ProgressBar>("%XpProgress");
            levelLabel = GetNode<Label>("%Level");
            patentLabel = GetNode<Label>("%Patent");
        }

        public void UpdateStats(Stats stats)
        {
            xpProgressbar.Value = stats.Xp;
            xpProgressbar.MaxValue = stats.CurrentMaxXp;
            levelLabel.Text = stats.Level.ToString();
            patentLabel.Text = stats.Patent;
        }
    }
}
