using Game.Items;
using Godot;
using System;

namespace Game
{
    [GlobalClass, Tool]
    public partial class MinigameInfo : Resource
    {
        public int? s, a, b, c;
        public enum MinigameDifficulty
        {
            Easy,
            Medium,
            Hard
        }

        [Export]
        public string MinigameName { get; set; }

        [Export]
        public MinigameDifficulty Difficulty { get; set; } = 0;

        [Export(PropertyHint.File, "*.tscn,*scn")]
        public string ScenePath { get; set; }

        [Export]
        public Item Prize { get; set; }

        [ExportGroup("XP per Grade", "Grade")]
        [Export]
        public int GradeSPoints
        {
            get => s ?? Difficulty switch
            {
                MinigameDifficulty.Easy => 1000,
                MinigameDifficulty.Medium => 1250,
                MinigameDifficulty.Hard => 1500,
                _ => 0
            };
            set => s = value;
        }

        [Export]
        public int GradeAPoints
        {
            get => a ?? Difficulty switch
            {
                MinigameDifficulty.Easy => 750,
                MinigameDifficulty.Medium => 1000,
                MinigameDifficulty.Hard => 1250,
                _ => 0
            };
            set => a = value;
        }

        [Export]
        public int GradeBPoints
        {
            get => b ?? Difficulty switch
            {
                MinigameDifficulty.Easy => 500,
                MinigameDifficulty.Medium => 750,
                MinigameDifficulty.Hard => 1000,
                _ => 0
            };
            set => b = value;
        }

        [Export]
        public int GradeCPoints
        {
            get => c ?? Difficulty switch
            {
                MinigameDifficulty.Easy => 250,
                MinigameDifficulty.Medium => 500,
                MinigameDifficulty.Hard => 750,
                _ => 0
            };
            set => c = value;
        }
    }
}
