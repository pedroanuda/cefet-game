using Godot;
using System;

namespace Game
{
    public enum TransitionStyleEnum
    {
        Circle,
        SlideFade
    }

    public partial class TransitionScene : Node2D
    {
        [Export(PropertyHint.Enum, "Circle,Slide Fade")]
        public TransitionStyleEnum TransitionStyle { get; set; } = TransitionStyleEnum.Circle;

        [Export(PropertyHint.Range, "0,20,or_greater")]
        public float MinimumTimeOnScreen = 0;

        [Export]
        public string LoadingMessage 
        {
            get => _loadingMessage;
            set
            {
                _loadingMessage = value;
                GetNode<Label>("%MessageLabel").Text = value;
            }
        }

        public bool LoadingMessageVisibility
        {
            get => GetNode<Control>("%MessageContainer").Visible;
        }

        public Sprite2D Sprite { get => TransitionStyle switch
        {
            TransitionStyleEnum.Circle => GetNode<Sprite2D>("%TransitionSprite"),
            TransitionStyleEnum.SlideFade => GetNode<Sprite2D>("%TransitionSprite2"),
            _ => null
        }; }

        private string _loadingMessage;
    }
}
