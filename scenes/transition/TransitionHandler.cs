using Godot;
using System;

namespace Game
{
    public partial class TransitionHandler : Node
    {
        [Signal] public delegate void TransitionInHalfEventHandler();

        [Export] public Vector2 StartingPosition { get; set; }
        private TransitionScene TransitionScene { get; set; }
        private AnimationPlayer TransitionPlayer { get; set; }
        private Sprite2D AnimationSprite { get; set; }

        public void Play(StringName animationName, string message = null, float timeOnScreen = 0)
        {
            if (!TransitionScene.IsInsideTree()) GetTree().Root.AddChild(TransitionScene);
            TransitionPlayer = TransitionScene.GetNode<AnimationPlayer>("%AnimationPlayer");
            AnimationSprite = TransitionScene.Sprite;
            VerifyStyle(animationName);

            if (TransitionPlayer.HasAnimation(animationName))
            {
                // Starting animations.
                if (animationName.ToString().EndsWith("start"))
                {
                    if (TransitionScene.TransitionStyle is TransitionStyleEnum.Circle)
                    {
                        TransitionScene.GetNode<Sprite2D>("%TransitionSprite").GlobalPosition = StartingPosition;
                    }

                    AnimationSprite.Show();
                    TransitionPlayer.Play(animationName);

                    void onFinish(StringName animName)
                    {
                        EmitSignal(SignalName.TransitionInHalf);

                        if (timeOnScreen > 1 && message is not null)
                        {
                            TransitionScene.LoadingMessage = message;
                            TransitionPlayer.Play("show_message");
                        }

                        TransitionPlayer.AnimationFinished -= onFinish;
                    };

                    TransitionPlayer.AnimationFinished += onFinish;
                }

                // Ending animations.
                else if (animationName.ToString().EndsWith("end"))
                {
                    if (TransitionScene.TransitionStyle == TransitionStyleEnum.Circle)
                    AnimationSprite.GlobalPosition = TransitionScene.GetNode<Control>("%PositionGuide").GetViewportRect().Size / 2;

                    // Hiding message if visible.
                    if (TransitionScene.LoadingMessageVisibility)
                    {
                        void callback(StringName anim)
                        {
                            TransitionPlayer.Play(animationName);
                            TransitionPlayer.AnimationFinished -= callback;
                            TransitionPlayer.AnimationFinished += onFinish;
                        }

                        TransitionPlayer.AnimationFinished += callback;
                        TransitionPlayer.Play("hide_message");
                    }
                    else
                    {
                        TransitionPlayer.Play(animationName);
                        TransitionPlayer.AnimationFinished += onFinish;
                    }

                    void onFinish(StringName animName)
                    {
                        AnimationSprite.Hide();
                        TransitionPlayer.AnimationFinished -= onFinish;
                    }
                }
            }
        }

        private void VerifyStyle(string anim_name)
        {
            if (anim_name.StartsWith("change_to_minigame"))
                TransitionScene.TransitionStyle = TransitionStyleEnum.Circle;
            else
                TransitionScene.TransitionStyle = TransitionStyleEnum.SlideFade;
        }

        public override void _Ready()
        {
            TransitionScene = ResourceLoader
                .Load<PackedScene>("res://scenes/transition/TransitionScene.tscn")
                .Instantiate<TransitionScene>();
        }
    }
}
