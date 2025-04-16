using Godot;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Linq;

namespace Game.Gameplay
{
    [Tool]
    public partial class InteractionArea : Area2D
    {
        [Export]
        public Node2D Interactor { get; set; }

        [Export]
        public StringName InteractionAction { get; set; } = "interact";

        private List<InteractableArea> nearbyInteractables = new();
        private InteractableArea _selectedInteractable;
        private Node2D interactionCursor;

        private InteractableArea SelectedInteractable
        {
            get => _selectedInteractable;
            set
            {
                if (_selectedInteractable == value) return;
                _selectedInteractable = value;
                UpdateCursor();
            }
        }

        private void OnAreaEntered(Area2D area)
        {
            if (area is not InteractableArea) return;

            var iArea = area as InteractableArea;
            nearbyInteractables.Add(iArea);
            SelectedInteractable ??= iArea;
        }

        private void OnAreaExited(Area2D area)
        {
            if (area is not InteractableArea) return;

            var iArea = area as InteractableArea;
            iArea.StopInteraction(Interactor);
            nearbyInteractables.Remove(iArea);
        }

        private void GetNearestInteractable()
        {
            if (nearbyInteractables.Count > 0)
            {
                var minimumDistance = nearbyInteractables[0].GlobalPosition.DistanceTo(Interactor.GlobalPosition);
                InteractableArea nearest = nearbyInteractables[0];

                nearbyInteractables.ForEach(interactable =>
                {
                    var distance = interactable.GlobalPosition.DistanceTo(Interactor.GlobalPosition);
                    if (distance < minimumDistance)
                    {
                        minimumDistance = distance;
                        nearest = interactable;
                    }
                });
                if (nearest is not null && nearest != SelectedInteractable)
                    SelectedInteractable = nearest;
                return;
            }
            if (SelectedInteractable is NpcInteractableArea npcArea)
                npcArea.GetParent<Entities.Npc>().PlayAnimation("idle");
            SelectedInteractable = null;
        }

        private void UpdateCursor()
        {
            if (SelectedInteractable is null)
            {
                interactionCursor?.QueueFree();
                interactionCursor = null;
                return;
            }

            if (SelectedInteractable is NpcInteractableArea npcArea)
            {
                var npc = npcArea.GetParent<Entities.Npc>();
                if (npcArea.GetParent<Entities.Npc>().HasSelectState)
                    npc.PlayAnimation("idle_selected");
                return;
            }

            var changePosition = () => {
                interactionCursor.GlobalPosition = SelectedInteractable.GlobalPosition;
                if (interactionCursor is Sprite2D i) i.Offset = SelectedInteractable.SelectorOffset;
            };

            if (interactionCursor is not null)
            {
                changePosition();
                interactionCursor.Reparent(SelectedInteractable);
            }
            else
            {
                interactionCursor = GD.Load<PackedScene>("res://scenes/entities/InteractionSelector.tscn")
                    .Instantiate<Node2D>();
                SelectedInteractable.AddChild(interactionCursor);
                changePosition();
            }

            
        }

        public override void _Ready()
        {
            if (Engine.IsEditorHint() && Interactor is null)
                Interactor = GetParent<Node2D>();
        }

        public override void _Process(double delta)
        {
            if (Engine.IsEditorHint()) return;

            GetNearestInteractable();
            if (Input.IsActionJustReleased(InteractionAction) &&
                SelectedInteractable is not null && ((Player)Interactor).AllowActions)
            {
                SelectedInteractable.StartInteraction(Interactor);
            }

            if (Interactor is Player p && interactionCursor is not null)
                if (!p.AllowActions) interactionCursor.Visible = false;
                else interactionCursor.Visible = true;
        }
    }
}
