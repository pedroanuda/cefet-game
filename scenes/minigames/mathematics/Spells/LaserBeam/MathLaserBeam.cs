using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Minigames
{
    public partial class MathLaserBeam : Node2D
    {
        [Export]
        public Vector2 SpellStartingLocation 
        { 
            get => _spellStLocation;
            set
            {
                _spellStLocation = value;
                _laserOrigin.GlobalPosition = SpellStartingLocation;
            }
        }

        [Export]
        public Color LaserColor { get; set; } = new("#e4453e");

        private AnimatedSprite2D _laserOrigin;
        private Node2D _beamsNode;
        private Dictionary<MathEnemy, Node2D> _bindedLasers = new();
        private Dictionary<Node2D, bool> _reachedLasers = new();
        private Vector2 _spellStLocation;
        private AnimationPlayer _animPlayer;

        private void OnHitTime()
        {
            foreach (var enemy in _bindedLasers.Keys)
                if (_reachedLasers[_bindedLasers[enemy]] && !enemy.Dead)
                {
                    enemy.TakeDamage(25, false);
                    enemy.Slow(.25, GetNode<Timer>("HitTimer").WaitTime);
                }
        }

        private void OnFreedomTime()
        {
            foreach (var enemy in _bindedLasers.Keys)
                enemy.Modulate = new Color("#FFFFFF");
            QueueFree();
        }

        public override void _Ready()
        {
            _laserOrigin = GetNode<AnimatedSprite2D>("LaserOrigin");
            _laserOrigin.Modulate = LaserColor;
            _laserOrigin.Position = SpellStartingLocation;
            _beamsNode = GetNode<Node2D>("Beams");
            _beamsNode.Modulate = LaserColor;
            _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        }

        public override void _PhysicsProcess(double delta)
        {
            var siblingNodes = GetParent().GetChildren().ToArray();
            foreach (var child in siblingNodes)
            {
                if (child is not MathEnemy enemy || enemy.Dead || (_bindedLasers.Count > 0 && !_bindedLasers.ContainsKey(enemy))) break;

                var distance = _laserOrigin.Position.DistanceTo(ToLocal(enemy.Position));
                if (!_bindedLasers.ContainsKey(enemy))
                {
                    var beam = (Node2D) GetNode<Node2D>("Beams/Beam").Duplicate();
                    beam.Visible = true;
                    
                    beam.GlobalPosition = ToLocal(_laserOrigin.GlobalPosition);
                    _beamsNode.AddChild(beam);
                    _bindedLasers.Add(enemy, beam);
                    _reachedLasers.Add(beam, false);
                    enemy.Died += () =>
                    {
                        beam.QueueFree();
                        _reachedLasers.Remove(beam);
                        _bindedLasers.Remove(enemy);
                    };
                }

                var fragment = _bindedLasers[enemy].GetNode<Sprite2D>("Fragment");
                var hitFragment = _bindedLasers[enemy].GetNode<AnimatedSprite2D>("HitFragment");
                var angle = _bindedLasers[enemy].GlobalPosition.AngleToPoint(enemy.GlobalPosition);
                var hasLaserReached = _reachedLasers[_bindedLasers[enemy]];
                var laserSpeed = (float) (15 * delta);
                _bindedLasers[enemy].GlobalRotation = angle + (float)(Math.PI/2);

                if (!hasLaserReached)
                {
                    if ((fragment.GlobalScale.Y) > (distance / 64 - laserSpeed))
                    {
                        fragment.GlobalScale = new Vector2(2, distance / 64);
                        _reachedLasers[_bindedLasers[enemy]] = true;
                        hitFragment.Show();
                        if (!_animPlayer.IsPlaying())
                            _animPlayer.Play("ely");
                    }
                    else fragment.GlobalScale = new Vector2(2, fragment.GlobalScale.Y + laserSpeed);

                    hitFragment.Position = new Vector2(hitFragment.Position.X, -distance);
                }
                else
                {
                    enemy.Modulate = MathEnemy.HitColor;
                    fragment.GlobalScale = new Vector2(2, distance / 64);
                }
            }
        }
    }
}
