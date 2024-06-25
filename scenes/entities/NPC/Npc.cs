using Game.Gameplay;
using Godot;
using Godot.Collections;
using System;

namespace Game.Entities
{
    [Tool]
    public partial class Npc : CharacterBody2D
    {
        private Character _characterInfo;
        private AnimatedSprite2D _animatedSprite;

        [Export]
        public Character CharacterInfo {
            get => _characterInfo;
            set
            {
                if (value is Character character)
                {
                    _characterInfo = character;
                    if (_animatedSprite is null) _animatedSprite = GetNode<AnimatedSprite2D>("Sprites");
                    _animatedSprite.SpriteFrames = character.SpriteAnimations;
                    _animatedSprite.Offset = character.SpritesOffset;
                    _animatedSprite.Play();
                }
                else
                {
                    _characterInfo = null;
                    if (_animatedSprite is null) return;
                    _animatedSprite.SpriteFrames = null;
                    _animatedSprite.Offset = Vector2.Zero;
                }
            }
        }

        [Export]
        public DialogueCollection Dialogues { get; set; }

        [Export]
        public Control InteractionUI { get; set; }

        public override void _Ready()
        {
            _animatedSprite = GetNode<AnimatedSprite2D>("Sprites");
        }

        // public override void _Process(double delta)
        // {
        //     _animatedSprite ??= GetNode<AnimatedSprite2D>("Sprites");
        // }
    }
}
