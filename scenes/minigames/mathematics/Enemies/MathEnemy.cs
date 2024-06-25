using Godot;
using System;
using System.Collections.Generic;

namespace Game.Minigames
{
    [GlobalClass, Tool]
    public partial class MathEnemy : CharacterBody2D
    {
        [Signal]
        public delegate void DiedEventHandler();

        private int _health = 20;
        protected AnimatedSprite2D _enemySprite;
        protected NavigationAgent2D _navigationAgent;

        [Export]
        public int Health 
        { 
            get => _health;
            set
            {
                if (value > 0)
                    _health = value;
                else _health = 0;
            }
        }

        [Export(PropertyHint.Range, "0,100")]
        public float Speed { get; set; }

        [Export]
        public int Damage { get; set; }

        [Export]
        public Vector2 Destination { get; set; }

        private Vector2 MovementTarget 
        {
            get => _navigationAgent is null ? Vector2.Zero : _navigationAgent.TargetPosition;
            set {
                if (_navigationAgent is not null)
                    _navigationAgent.TargetPosition = value;
            }
        }

        public virtual void TakeDamage(int amount) 
        { 
            Health -= amount;

            if (Health <= 0)
            {
                EmitSignal(SignalName.Died);
                QueueFree();
            }
        }

        public override void _Ready()
        {
            _enemySprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");
            _navigationAgent = GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");
            _enemySprite.SpeedScale = Speed / 80;

            if (!Engine.IsEditorHint()) Callable.From(ActorSetup).CallDeferred();
        }

        public override void _Process(double delta)
        {
            if (!Engine.IsEditorHint()) return;

            _enemySprite ??= GetNodeOrNull<AnimatedSprite2D>("Sprite");
            _navigationAgent ??= GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_navigationAgent.IsNavigationFinished() || Engine.IsEditorHint()) return;


            var currentPosition = GlobalTransform.Origin;
            var nextPosition = _navigationAgent.GetNextPathPosition();

            Velocity = currentPosition.DirectionTo(nextPosition) * Speed;
            MoveAndSlide();
        }

        public override string[] _GetConfigurationWarnings()
        {
            var warnings = new List<string>();
            if (_enemySprite is null) warnings.Add("Sprite node \"Sprite\" is missing.");
            if (_navigationAgent is null) warnings.Add("Navigation Agent missing. Create a \"NavigationAgent2D\" node.");

            return warnings.ToArray();
        }

        private async void ActorSetup()
        {
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
            MovementTarget = Destination;
        }
    }
}
