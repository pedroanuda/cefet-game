using Game.Entities;
using Game.Gameplay;
using Game.UI;
using Godot;
using System;

namespace Game.Gameplay
{
    [GlobalClass]
    public partial class NpcInteractableArea : InteractableArea
    {
        [Export(PropertyHint.Enum, "Talk")]
        public int Interaction { get; set; } = 0;
        protected bool interacting = false;

        public override void StartInteraction(Node2D interactor)
        {
            if (interacting) return;
            var npc = GetParent<Npc>();
            interacting = true;
            
            if (Interaction == 0 && npc.InteractionUI is DialogueUi ui
                && interactor is Player player)
            {
                player.AllowActions = false;
                ui.Open(npc.Dialogues, () => {
                    player.AllowActions = true;
                    interacting = false;
                });
            }
        }
    }
}
