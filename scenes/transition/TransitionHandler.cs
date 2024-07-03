using Godot;
using System;

namespace Game
{
    public partial class TransitionHandler : Node
    {
        [Signal] public delegate void TransitionInHalfEventHandler();

        [Export] public Vector2 StartingPosition { get; set; }
        private Node2D TransitionScene { get; set; }
        private AnimationPlayer TransitionPlayer { get; set; }
        private Sprite2D AnimationSprite { get; set; }

        public void Play(StringName animationName)
        {
            if (!TransitionScene.IsInsideTree()) GetTree().Root.AddChild(TransitionScene);
            TransitionPlayer = TransitionScene.GetNode<AnimationPlayer>("%AnimationPlayer");
            AnimationSprite = TransitionScene.GetNode<Sprite2D>("%TransitionSprite");

            if (TransitionPlayer.HasAnimation(animationName))
            {
                if (animationName.ToString().EndsWith("start"))
                {
                    AnimationSprite.Position = StartingPosition;
                    AnimationSprite.Show();
                    TransitionPlayer.Play(animationName);

                    void onFinish(StringName animName)
                    {
                        EmitSignal(SignalName.TransitionInHalf);
                        TransitionPlayer.AnimationFinished -= onFinish;
                    };

                    TransitionPlayer.AnimationFinished += onFinish;
                }
                else if (animationName.ToString().EndsWith("end"))
                {
                    AnimationSprite.GlobalPosition = new Vector2(1280, 720) / 2;
                    TransitionPlayer.Play(animationName);

                    void onFinish(StringName animName)
                    {
                        AnimationSprite.Hide();
                        TransitionPlayer.AnimationFinished -= onFinish;
                    }

                    TransitionPlayer.AnimationFinished += onFinish;
                }
            }
        }

        public override void _Ready()
        {
            TransitionScene = ResourceLoader
                .Load<PackedScene>("res://scenes/transition/TransitionScene.tscn")
                .Instantiate<Node2D>();
        }
    }
}
