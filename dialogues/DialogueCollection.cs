using Godot;
using Godot.Collections;
using System;

namespace Game.Gameplay
{
    [GlobalClass, Tool, Icon("res://src/engine_ui/dialogue_collection_icon.svg")]
    public partial class DialogueCollection : Resource
    {
        [Export]
        public Array<Dialogue> Dialogues { get; set; }
    }
}
