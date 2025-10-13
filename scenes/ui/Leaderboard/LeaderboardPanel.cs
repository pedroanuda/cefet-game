using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Game.UI
{
    public partial class LeaderboardPanel : PanelContainer
    {
        private static readonly List<(string Name, int Score)> _entries = new();
        private static int _playerCounter = 1;

        private VBoxContainer _entriesContainer;

        public override void _Ready()
        {
            _entriesContainer = GetNode<VBoxContainer>("%EntriesContainer");
            UpdateDisplay();
            Hide(); 
        }

        public static void AddScore(int score)
        {
            string playerName = $"Jogador {_playerCounter}";
            _entries.Add((playerName, score));
            _entries.Sort((a, b) => b.Score.CompareTo(a.Score));
            _playerCounter++;

            if (_entries.Count > 10)
                _entries.RemoveRange(10, _entries.Count - 10);
        }

        public void UpdateDisplay()
        {
            foreach (Node child in _entriesContainer.GetChildren())
                child.QueueFree();

            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];

                var label = new Label
                {
                    Text = $"{i + 1}. {entry.Name} - ({entry.Score} pontos)",
                    HorizontalAlignment = HorizontalAlignment.Center,
                };

                _entriesContainer.AddChild(label);
            }
        }


    }
}
