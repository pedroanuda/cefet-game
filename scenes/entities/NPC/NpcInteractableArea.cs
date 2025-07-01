using CefetGame.Misc;
using CefetGame.Ui;
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

        protected Node2D currentInteractor;
        protected bool interacting = false;
        protected bool _hasSelectedState = false;

        public override void StartInteraction(Node2D interactor)
        {
            if (interacting) return;
            var npc = GetParent<Npc>();
            interacting = true;
            currentInteractor = interactor;
            
            if (Interaction == 0 && npc.InteractionUI is NewDialogueUi ui
                && interactor is Player player)
            {
                player.AllowActions = false;
                var handler = GetNode<TransitionHandler>("/root/TransitionHandler");
                handler.StartingPosition = npc.GlobalPosition;
                DialogueSystem.Instance.StartDialogue(ui, npc.DialoguePath, npc.CharacterInfo);
                DialogueSystem.Instance.DialogueFinished += OnDialogueFinished;
            }
        }
        public override void StopInteraction(Node2D interactor)
        {
            base.StopInteraction(interactor);
            _hasSelectedState = false;
            interacting = false;
            currentInteractor = null;
        }

        private void OnDialogueFinished()
        {
            if (currentInteractor is Player player)
            {
                player.AllowActions = true;
            }
            interacting = false;
            DialogueSystem.Instance.DialogueFinished -= OnDialogueFinished;
        }
    }
}
