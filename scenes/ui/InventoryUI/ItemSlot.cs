using Game.Items;
using Godot;
using System;

namespace Game.UI
{
    public partial class ItemSlot : Control
    {
        protected TextureRect _icon;
        protected TextureButton _button;

        [Signal]
        public delegate void PressedEventHandler(Item item);

        public override void _Ready()
        {
            _icon = GetNode<TextureRect>("%ItemIcon");
            _button = GetNode<TextureButton>("%Button");
        }

        public virtual void DisplayItem(Item item)
        {
            _icon.Texture = item.Icon;
            _button.Pressed += () => EmitSignal(SignalName.Pressed, item);
        }
    }
}
