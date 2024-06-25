using Godot;
using System;

namespace Game.Items
{
    public enum ItemCategory
    {
        Generic,
        Backpack,
        Hat,
        MinigameSpecific
    }

    [GlobalClass, Tool]
    public partial class Item : Resource
    {
        [Export(PropertyHint.Range, "1,60,1,or_greater")]
        public int Id { get; set; } = 1;
        [Export(PropertyHint.LocalizableString)]
        public Godot.Collections.Dictionary<string, string> Name { get; set; }

        [Export(PropertyHint.LocalizableString)]
        public Godot.Collections.Dictionary<string, string> Description { get; set; }

        [Export]
        public int StackSize { get; set; } = 1;

        [Export]
        public ItemCategory SlotType { get; set; } = 0;

        [Export]
        public Texture2D Icon { get; set; }
    }
}
