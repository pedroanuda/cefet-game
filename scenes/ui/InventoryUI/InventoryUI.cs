using Game;
using Game.Items;
using Game.UI;
using Godot;

namespace Game.UI
{
	public partial class InventoryUI : Control
	{
		private TextureRect _displayedImage;
		private GridContainer _specialItemsGrid;
		private GridContainer _itemsGrid;
		private RichTextLabel _itemInfo;
		private int _selectedItemId;

		[Export]
		private PackedScene _iconSlotScene;

		public override void _Ready()
		{
			_itemsGrid = GetNode<GridContainer>("%ItemsGrid");
			_specialItemsGrid = GetNode<GridContainer>("%SpecialItemsGrid");
			_displayedImage = GetNode<TextureRect>("%DisplayedImage");
			_itemInfo = GetNode<RichTextLabel>("%ItemInfo");
		}

		public void Select(Item item)
		{
			if (item.Id == _selectedItemId) return;

			_displayedImage.Texture = item.Icon;
			_itemInfo.Clear();
			_itemInfo.Text = "";
			_itemInfo.AppendText($"[font_size=30][center]{item.Name["pt_BR"]}[/center][/font_size]\n");
			_itemInfo.AppendText($"[font_size=20]{item.Description["pt_BR"]}[/font_size]");
			_selectedItemId = item.Id;
		}

		public void Open(Inventory inventory)
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
			Show();

			foreach (var item in inventory.GetItems())
			{
				ItemSlot slot;
				if (item.SlotType == ItemCategory.Generic)
				{
					slot = _iconSlotScene.Instantiate<ItemSlot>();
					_itemsGrid.AddChild(slot);
					_itemsGrid.GetParent<Container>().Visible = true;
				} else slot = item.SlotType switch
				{
					ItemCategory.Backpack => _specialItemsGrid.GetNode<SpecialItemSlot>("BackpackSlot"),
					_ => null
				};

				slot.DisplayItem(item);
				slot.Pressed += Select;
				if (_selectedItemId == 0 || !inventory.Exists(_selectedItemId)) Select(item);

				GetNode<Container>("%Information").Visible = true;
				GetNode<Container>("%BlankInformation").Visible = false;
			}
		}

		public void Close()
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
			Hide();

            foreach (var child in _itemsGrid.GetChildren())
                child.QueueFree();

			_itemsGrid.GetParent<Container>().Visible = false;
        }
	}
}
