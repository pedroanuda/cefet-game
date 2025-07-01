using Godot;
using System;
using System.Linq;

namespace Game.Minigames
{
    public partial class MathLaserBeam : Node2D
    {
        [Export]
        public Vector2 SpellStartingLocation
        {
            get => _spellStartingLocation;
            set
            {
                _spellStartingLocation = value;
                if (_laserOrigin != null)
                    _laserOrigin.GlobalPosition = _spellStartingLocation;
            }
        }

        [Export]
        public Color LaserColor { get; set; } = new Color("#e4453e");

        private Vector2 _spellStartingLocation;
        private AnimatedSprite2D _laserOrigin;
        private Node2D _beam;
        private Sprite2D _fragment;
        private AnimatedSprite2D _hitFragment;
        private AnimationPlayer _animPlayer;
        private Timer _hitTimer;

        private MathEnemy _currentEnemy;
        private bool _laserReached;

        public override void _Ready()
        {
            _laserOrigin = GetNode<AnimatedSprite2D>("LaserOrigin");
            _laserOrigin.Modulate = LaserColor;
            _laserOrigin.Position = SpellStartingLocation;

            _beam = GetNode<Node2D>("Beam");
            _beam.Visible = false;
            _beam.Modulate = LaserColor;

            _fragment = _beam.GetNode<Sprite2D>("Fragment");
            _hitFragment = _beam.GetNode<AnimatedSprite2D>("HitFragment");
            _hitFragment.Hide();

            _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _hitTimer = GetNode<Timer>("HitTimer");

            TrySetNextEnemy();
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_currentEnemy == null || _currentEnemy.Dead)
            {
                TrySetNextEnemy();
                return;
            }

            var distance = _laserOrigin.GlobalPosition.DistanceTo(_currentEnemy.GlobalPosition);
            var laserSpeed = (float)(15 * delta);

            var angle = _laserOrigin.GlobalPosition.AngleToPoint(_currentEnemy.GlobalPosition);
            _beam.GlobalRotation = angle + (float)(Math.PI / 2);

            _beam.GlobalPosition = _laserOrigin.GlobalPosition;
            _beam.Visible = true;

            if (!_laserReached)
            {
                if (_fragment.Scale.Y * 64 >= distance - laserSpeed * 64)
                {
                    _fragment.Scale = new Vector2(2, distance / 64);
                    _laserReached = true;
                    _hitFragment.Show();
                    _hitFragment.Position = new Vector2(_hitFragment.Position.X, -distance);

                    if (!_animPlayer.IsPlaying())
                        _animPlayer.Play("ely");

                    _hitTimer.Start();
                }
                else
                {
                    _fragment.Scale = new Vector2(2, _fragment.Scale.Y + laserSpeed);
                }

                _hitFragment.Position = new Vector2(_hitFragment.Position.X, -_fragment.Scale.Y * 64);
            }
            else
            {
                _fragment.Scale = new Vector2(2, distance / 64);
                _currentEnemy.Modulate = MathEnemy.HitColor;
            }
        }

        private void TrySetNextEnemy()
        {
            _beam.Visible = false;
            _laserReached = false;
            _fragment.Scale = new Vector2(2, 0);
            _hitFragment.Hide();

            var enemies = GetParent().GetChildren()
                .OfType<MathEnemy>()
                .Where(e => !e.Dead)
                .ToList();

            if (enemies.Count > 0)
            {
                _currentEnemy = enemies.Last();
                _currentEnemy.Died += OnEnemyDied;
            }
            else
            {
                QueueFree(); 
        }

        private void OnEnemyDied()
        {
            _currentEnemy.Died -= OnEnemyDied;
            _currentEnemy = null;
            TrySetNextEnemy();
        }

        private void OnHitTime()
        {
            if (_laserReached && _currentEnemy != null && !_currentEnemy.Dead)
            {
                _currentEnemy.TakeDamage(25, false);
                _currentEnemy.Slow(0.25f, _hitTimer.WaitTime);
            }
        }

        private void OnFreedomTime()
        {
            if (_laserReached && _currentEnemy != null)
                _currentEnemy.Modulate = new Color("#FFFFFF");

            QueueFree();
        }
    }
}
