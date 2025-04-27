using Godot;
using System;

namespace Game.Gameplay
{
    [GlobalClass, Icon("res://misc/engine_ui/dialogue_icon.svg")]
    public partial class Dialogue : Resource
    {
        [Export(PropertyHint.EnumSuggestion, "{character},{player}")]
        public string Title { get; set; } = "";

        [Export(PropertyHint.MultilineText)]
        public string Body { get; set; }
    }
}
