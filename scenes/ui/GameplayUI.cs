using Godot;
using Game.Gameplay;
using System;

namespace Game.UI
{
    public partial class GameplayUI : Node
    {
        [Export] private Player player;
        private InventoryUI inventoryUI;
        private PauseScreen pauseScreen;
        private MainGameUi mainGameUi;
        private bool inventoryOpened = false;

        public override void _Ready()
        {
            inventoryUI = GetNode<InventoryUI>("InventoryUi");
            pauseScreen = GetNode<PauseScreen>("PauseScreen");
            mainGameUi = GetNode<MainGameUi>("MainGameUi");

            mainGameUi.UpdateStats(player.Stats);
            player.Stats.Changed += () => mainGameUi.UpdateStats(player.Stats);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (Input.IsActionJustReleased("inventory"))
                if (!inventoryOpened)
                {
                    inventoryUI.Open(player.Inventory);
                    player.AllowActions = false;
                    inventoryOpened = true;
                }
                else
                {
                    inventoryUI.Close();
                    player.AllowActions = true;
                    inventoryOpened = false;
                }
            else if (@event.IsActionReleased("go_back"))
                pauseScreen.HandlePause();

        }
    }
}
