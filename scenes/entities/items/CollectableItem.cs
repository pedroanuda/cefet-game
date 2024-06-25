using Game.Items;
using Godot;
using System;

namespace Game.Gameplay
{
	[Tool]
	public partial class CollectableItem : Node2D
	{
		private CollectableItemArea _area;
		private Sprite2D _sprite;
		private Item _item;

		[Export]
		public Item Item { 
			get => _item;
			set
			{
				_item = value;
				if (_sprite is not null)
					_sprite.Texture = _item.Icon;
			}
		}

		public override void _Ready()
		{
			_area = GetNodeOrNull<CollectableItemArea>("CollectableItemArea");
			if (_area != null) _area.Item = Item;
			_sprite = GetNode<Sprite2D>("Sprite2D");
			_sprite.Texture = Item.Icon;
		}
	}
}
