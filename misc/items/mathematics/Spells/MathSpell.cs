using Godot;
using System;

namespace Game.Items
{
    [GlobalClass]
    public partial class MathSpell : Item
    {
        [Export(PropertyHint.File, "*.tscn,*.scn")]
        public string SpellScenePath { get; set; }

        [Export(PropertyHint.PlaceholderText, "Can be empty.")]
        public PackedScene PreviewScene { get; set; }

        [ExportGroup("Craft Recipe", "Craft")]
        [Export]
        public Godot.Collections.Array<Item> CraftRecipe { get; set; }
    }
}
