using Godot;
using System;
using System.Collections.Generic;

namespace Game.Minigames
{
    public partial class MathFireball : Node2D
    {
        [Export] public Vector2 SurgementPoint {
            get => _surgement;
            set {
                _surgement = value;
                _core.GlobalPosition = value;
            }
        }
        [Export] public Vector2 Destiny { get; set; }
        [Export] public float Speed { get; set; } = 24;

        private List<MathEnemy> _enemies = new();
        private Timer _timer;
        private AnimatedSprite2D _fireBallSprite;
        private AnimatedSprite2D _fireCircleTopSprite;
        private AnimatedSprite2D _fireCircleBottomSprite;
        private Node2D _core;
        private Vector2 _surgement;
        private bool _completed = false;
        private bool _surging = false;
        private bool _vanishing = false;

        public override void _Ready()
        {
            _core = GetNode<Node2D>("Core");
            _fireCircleTopSprite = GetNode<AnimatedSprite2D>("Core/fire_circle_top");
            _fireCircleBottomSprite = GetNode<AnimatedSprite2D>("Core/fire_circle_bottom");
            _fireBallSprite = GetNode<AnimatedSprite2D>("Core/ball");
            _timer = GetNode<Timer>("TimeOn");
            _timer.Timeout += OnTimeOut;

            _core.GlobalPosition = ToLocal(SurgementPoint);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_completed) return;
            var direction = _core.GlobalPosition.DirectionTo(Destiny);
            var distance = _core.GlobalPosition.DistanceTo(Destiny);
            var possibleNewPosition = _core.Position + (direction * Speed * 12 * (float)delta);

            if (distance == 0 || ToGlobal(possibleNewPosition).DistanceTo(Destiny) > distance)
            {
                _core.Position = possibleNewPosition;
                _completed = true;
                Ignite();
            }
            else _core.Position = possibleNewPosition;
        }

        private void Ignite()
        {
            _fireBallSprite.Hide();
            _fireCircleTopSprite.Show();
            _fireCircleTopSprite.Play("surge");
            _fireCircleBottomSprite.Show();
            _fireCircleBottomSprite.Play("surge");
            var particles = GetNode<CpuParticles2D>("Core/CPUParticles2D");
            particles.Emitting = true;
            particles.Reparent(GetParent());
            particles.Finished += () => particles.QueueFree();
            _surging = true;
        }

        private void OnTimeOut()
        {
            _vanishing = true;
            _fireCircleTopSprite.Play("disappearing");
            _fireCircleBottomSprite.Play("disappearing");
        }

        private void OnFireCircleAnimationFinish()
        {
            if (_surging)
            {
                CallDeferred(MethodName.DamageEnemies);
                _fireCircleTopSprite.Play("default");
                _fireCircleBottomSprite.Play("default");
                _timer.Start();
                _surging = false;
            }
            else if (_vanishing)
                QueueFree();
        }

        private void DamageEnemies()
        {
            foreach (var en in _enemies.ToArray())
                en.TakeDamage(75);
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body is not MathEnemy enemy) return;
            _enemies.Add(enemy);
            enemy.Died += () => _enemies.Remove(enemy);
        }

        private void OnBodyExited(Node2D body)
        {
            if (body is not MathEnemy enemy) return;
            _enemies.Remove(enemy);
        }
    }
}
