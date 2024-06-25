using Godot;
using System;

namespace Game.UI
{
    public partial class MenuButtons : Control
    {
        public enum ButtonsAlignmentEnum { Left, Center, Right }
        [Export] public ButtonsAlignmentEnum ButtonsAlignment { get; set;}
    }
}
