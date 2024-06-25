using Godot;
using System;
using System.Collections.Generic;

namespace Game.Minigames
{
    public partial class MathStorm : Node2D
    {
        [Export]
        public PackedScene LightningScene { get; set; }

        [Export(PropertyHint.Range, "0,200")]
        public float LightningHeightLimit { get; set; } = 200;

        private Timer _deletionTimer;
        private Timer _lightningTimer;
        private Node2D _lightningsNode;
        private AnimationPlayer _anPlayer;
        private readonly List<MathEnemy> enemiesInArea = new();

        public override void _Ready()
        {
            _deletionTimer = GetNode<Timer>("FallTimer");
            _lightningTimer = GetNode<Timer>("LightningTimer");
            _lightningsNode = GetNode<Node2D>("Lightnings");
            _anPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _deletionTimer.Timeout += () => _anPlayer.Play("disappearance");
        }

        private void OnAnimationFinished(StringName animationName)
        {
            if (animationName == "surgement")
            {
                _deletionTimer.Start();
                _lightningTimer.Start();
                GetNode<CpuParticles2D>("ParticlesEmissor").Emitting = true;
                OnLightningTiming();
            }
            else if (animationName == "disappearance")
                QueueFree();
        }

        private void OnBodyEnteredArea(Node2D body)
        {
            if (body is not MathEnemy enemy) return;
            enemiesInArea.Add(enemy);
        }

        private void OnBodyExitedArea(Node2D body)
        {
            if (body is not MathEnemy enemy) return;
            enemiesInArea.Remove(enemy);
        }

        private void OnLightningTiming()
        {
            foreach (var enemy in enemiesInArea)
            {
                var instance = LightningScene.Instantiate<AnimatedSprite2D>();
                var texture = instance.SpriteFrames.GetFrameTexture("default", 0);

                instance.Position = new Vector2(
                    ToLocal(enemy.GlobalPosition).X,
                    ToLocal(enemy.GlobalPosition).Y - (texture.GetHeight() / 2)
                );

                var lightningTop = instance.Position.Y - (texture.GetHeight() / 2);
                if (lightningTop < -LightningHeightLimit)
                {
                    var difference = -LightningHeightLimit - lightningTop;

                    instance.Scale = new Vector2(
                        instance.Scale.X,
                        instance.Scale.Y - (difference / texture.GetHeight() * instance.Scale.Y)
                    );

                    instance.Position = new Vector2(
                        instance.Position.X,
                        instance.Position.Y + difference
                    );
                }

                _lightningsNode.AddChild(instance);
                instance.Play("default");
                enemy.TakeDamage(35);
            }
        }
    }
}
