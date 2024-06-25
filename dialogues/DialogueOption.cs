using Godot;
using System;

namespace Game.Gameplay
{
    [GlobalClass]
    public partial class DialogueOption : Resource
    {
        [Export]
        public string OptionText { get; set; }

        [Export]
        public DialogueCollection CourseOfAction { get; set; }
    }
}
