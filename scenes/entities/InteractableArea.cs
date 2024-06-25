using Godot;
using System;

namespace Game.Gameplay
{
    [GlobalClass]
    public abstract partial class InteractableArea : Area2D
    {
        [Export]
        public StringName InteractionAction { get; set; } = "interact";

        [ExportGroup("Interaction Selector", "Selector")]
        [Export(PropertyHint.File, "*.tscn,*.scn")]
        public string SelectorScenePath { get; set; } = "res://scenes/entities/InteractionSelector.tscn";
        [Export] public Vector2 SelectorOffset { get; set; }


        public abstract void StartInteraction(Node2D interactor);
        public virtual void StopInteraction(Node2D interactor) {
            // Overridable Code
        }
    }
}
