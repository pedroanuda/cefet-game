using Game.Items;
using Godot;
using System;

namespace Game.Gameplay
{
    [GlobalClass]
    public partial class CollectableItemArea : InteractableArea
    {
        [Export]
        public Item Item { get; set; }

        public override void StartInteraction(Node2D interactor)
        {
            if (interactor is Player player) {
                player.Inventory.Add(Item);
                if (Item.SlotType == ItemCategory.Backpack)
                    player.Backpack = "Simple";

                var selector = GetNodeOrNull("InteractionSelector");
                if (selector is null) 
                {
                    GetParent().QueueFree();
                    return;
                }
                selector.Reparent(interactor);
                GetParent().QueueFree();
            }
        }
    }
}
