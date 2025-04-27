using Game.Items;
using Godot;
using System;

namespace Game.UI
{
    public partial class SpecialItemSlot : ItemSlot
    {
        private TextureRect _placeholder;

        [Export]
        public ItemCategory SlotType { get; set; }

        public override void _Ready()
        {
            _icon = GetNode<TextureRect>("%ItemIcon");
            _button = GetNode<TextureButton>("%Button");
            _placeholder = GetNode<TextureRect>("%PlaceholderImage");

            _button.Disabled = true;
            _placeholder.Texture = SlotType switch
            {
                ItemCategory.Backpack => GD.Load<Texture2D>("res://assets/ui/inventory/backpack_slot_placeholder.png"),
                _ => null
            };
        }

        public override void DisplayItem(Item item)
        {
            _button.Disabled = false;
            _placeholder.Visible = false;

            _icon.Texture = item.Icon;
            _button.Pressed += () => EmitSignal(SignalName.Pressed, item);
        }
    }
}
