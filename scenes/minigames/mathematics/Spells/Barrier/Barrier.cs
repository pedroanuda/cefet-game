using Godot;
using System;

namespace Game.Minigames
{
    public partial class Barrier : Node2D
    {
        private void OnTimerTimeout()
        {
            QueueFree();
        }
    }
}
