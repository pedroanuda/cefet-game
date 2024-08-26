using Godot;
using System;
using System.Collections.Generic;

namespace Game.Minigames {
    public partial class MathMeteor : CanvasLayer
    {
        public Camera2D Camera { get; set; }
        public Vector2 MeteorDestiny 
        { 
            get => _meteorDestiny;
            set 
            {
                _meteorDestiny = value;
                AdjustAnimation();
            }
        }
        private AnimationPlayer _animPlayer;
        private Vector2 _meteorDestiny = Vector2.Zero;

        public override void _Ready() {
            _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _animPlayer.AnimationFinished += OnHalfEnded;
        }

        private void HarmEnemies()
        {
            foreach(var child in GetParent().GetChildren())
                if (child is MathEnemy enemy)
                {
                    enemy.TakeDamage(250);
                }
        }

        private void OnHalfEnded(StringName animName) {
            switch (animName) {
                case "default":
                    _animPlayer.Play("end_default");
                    HarmEnemies();

                    var audioPlayer = GetNode<AudioStreamPlayer2D>("Core/AudioPlayer");
                    audioPlayer.Finished += audioPlayer.QueueFree;
                    audioPlayer.Reparent(GetParent());
                    audioPlayer.Play();

                    if (Camera is not null && Camera.HasMethod("shake"))
                    {
                        Camera.Call("shake");
                    }
                    break;

                case "end_default":
                    QueueFree();
                    break;
            }
        }

        private void AdjustAnimation()
        {
            if (_meteorDestiny == Vector2.Zero) return;

            var animation = _animPlayer.GetAnimation("default");
            var anim_track = animation.FindTrack("Core:position", Animation.TrackType.Value);

            animation.TrackSetKeyValue(anim_track, 1, _meteorDestiny);
        }
    }
}
