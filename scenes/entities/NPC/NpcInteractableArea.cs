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

        public bool HasSelectedState { get => _hasSelectedState; }

        protected bool interacting = false;
        protected bool _hasSelectedState = false;

        public override void StartInteraction(Node2D interactor)
        {
            if (interacting) return;
            var npc = GetParent<Npc>();
            interacting = true;
            
            if (Interaction == 0 && npc.InteractionUI is DialogueUi ui
                && interactor is Player player)
            {
                player.AllowActions = false;
                ui.Open(npc.Dialogues, async () => {
                    player.AllowActions = true;
                    await ToSignal(GetTree().CreateTimer(1), SceneTreeTimer.SignalName.Timeout);
                    interacting = false;
                }, npc);
            }
        }
        public override void StopInteraction(Node2D interactor)
        {
            base.StopInteraction(interactor);
            _hasSelectedState = false;
        }
    }
}
