using Godot;
using Godot.Collections;
using System;

namespace Game.Gameplay
{
    [GlobalClass]
    public partial class ChooseDialogue : Dialogue
    {
        [Export]
        public Array<DialogueOption> Options;
    }
}
