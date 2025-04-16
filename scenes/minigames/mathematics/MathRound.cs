using Godot;
using System;

namespace Game.Minigames {
    [GlobalClass]
    public partial class MathRound : Resource
    {
        [Export] public string RoundName { get; set; }
        [Export] public int EnemiesAmount { get; set; }
    }
}
