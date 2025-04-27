using Godot;
using Godot.Collections;
using System;

namespace Game.Gameplay
{
    public enum DialogueFinishing
    {
        None,
        ItemReceiving,
        SceneSwitch
    }

    [GlobalClass, Tool, Icon("res://misc/engine_ui/dialogue_collection_icon.svg")]
    public partial class DialogueCollection : Resource
    {
        [Export]
        public Array<Dialogue> Dialogues { get; set; }

        [Export]
        public DialogueFinishing EndingType { get; set; } = DialogueFinishing.None;

        [Export(PropertyHint.Expression)]
        public string EndingExpression { get; set; }
    }
}
