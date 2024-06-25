using Godot;
using System;
using System.Linq;

namespace Game
{
    [Tool, GlobalClass]
    public partial class SplashFrame : Control
    {
        // Signals
        [Signal] public delegate void OnFinishedEventHandler();

        // Transitions Names
        private static readonly string[] InTransitions = new string[] {
            "fadeIn",
            "slideRightIn"
        };
        private static readonly string[] OutTransitions = new string[]
        {
            "fadeOut",
            "slideRightOut"
        };

        // Fields
        private bool finishing = false;
        private bool _thingsFound = true;
        private Texture2D _splashImage;
        private Vector2 _splashScale = Vector2.One;
        private AnimationPlayer _animationPlayer;
        private AnimationPlayer _zoomPlayer;
        private string zoomAnimationName;

        [Export]
        public Texture2D SplashImage { 
            get => _splashImage;
            set {
                _splashImage = value;
                var spriteNode = FindChild("SplashSprite");
                if (spriteNode is Sprite2D sprite)
                    sprite.Texture = value;
            }
        }

        [Export(PropertyHint.Link)]
        public Vector2 SplashScale {
            get => _splashScale;
            set {
                _splashScale = value;
                GetNode<Sprite2D>("SplashSprite").Scale = _splashScale;
            }
        }

        [Export(PropertyHint.Range, "0.5,5,0.5,suffix:s")]
        public float TimeOnScreen { get; set; } = 1.5f;

        [Export]
        public bool Skippable { get; set; } = true;

        [ExportGroup("Transitions")]
        [Export(PropertyHint.Enum, "Fade In,Slide Right")]
        public int InTransition { get; set; }

        [Export(PropertyHint.Enum, "Fade Out,Slide Right")]
        public int OutTransition { get; set; }

        [Export(PropertyHint.Range, "0.5,2,0.5,suffix:s")]
        public float TransitionsDuration { get; set; } = 1f;

        [ExportGroup("Zoom")]
        [Export]
        public bool ZoomOn { get; set; } = false;
        [Export(PropertyHint.None, "suffix:px/s")]
        public float PixelsPerSecond { get; set; } = 1f;

        private void AdjustZoom()
        {
            var timeTaken = TimeOnScreen;
            var finalScale = new Vector2(
                SplashScale.X + (PixelsPerSecond * timeTaken),
                SplashScale.Y + (PixelsPerSecond * timeTaken)
            );
        
            var animation = new Animation();
            int trackIndex = animation.AddTrack(Animation.TrackType.Value);
            animation.TrackSetPath(trackIndex, "SplashSprite:scale");
            animation.Length = timeTaken;
        
            animation.TrackInsertKey(trackIndex, 0f, SplashScale);
            animation.TrackInsertKey(trackIndex, timeTaken, finalScale);
            
            _zoomPlayer.GetAnimationLibrary("").AddAnimation(zoomAnimationName, animation);
        }

        private async void Finish()
        {
            if (finishing) return;
            
            finishing = true;
            _animationPlayer.Play(OutTransitions[OutTransition], -1, 1 / TransitionsDuration);
            await ToSignal(_animationPlayer, AnimationPlayer.SignalName.AnimationFinished);

            EmitSignal(SignalName.OnFinished);
            QueueFree();
        }

        public async void Play()
        {
            finishing = false;
            Show();
            GetNode<Sprite2D>("SplashSprite").Modulate = new Color("#FFFFFF", 0);
            if (ZoomOn)
            {
                if (_zoomPlayer.HasAnimation(zoomAnimationName) && !_zoomPlayer.IsPlaying())
                    _zoomPlayer.Play(zoomAnimationName);
            }

            _animationPlayer.Play(InTransitions[InTransition], -1, 1 / TransitionsDuration);
            await ToSignal(GetTree().CreateTimer(TimeOnScreen - TransitionsDuration), SceneTreeTimer.SignalName.Timeout);
            Finish();
        }

        public override void _Ready()
        {
            if (Engine.IsEditorHint()) return;

            var sprite = GetNode<Sprite2D>("SplashSprite");
            sprite.Texture = SplashImage;
            sprite.Scale = SplashScale;

            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _zoomPlayer = GetNode<AnimationPlayer>("ZoomPlayer");
            zoomAnimationName = $"zoomIn{Name}";

            if (ZoomOn) AdjustZoom();
            Hide();
        }

        public override void _Input(InputEvent @event)
        {
            var isValidInput = @event is InputEventKey or InputEventJoypadButton or InputEventScreenTouch;
            if (isValidInput && Skippable && Visible)
                Finish();
        }

        public override void _Process(double delta)
        {
            if (!Engine.IsEditorHint()) return;

            var playerNullSituation = FindChild("AnimationPlayer") is null;
            if (playerNullSituation == _thingsFound)
            {
                _thingsFound = !playerNullSituation;
                UpdateConfigurationWarnings();
            }
        }

        public override string[] _GetConfigurationWarnings()
        {
            Godot.Collections.Array<string> warnings = new();

            if (!_thingsFound)
                warnings.Add("No AnimationPlayer found.");

            if (SplashImage is null)
                warnings.Add("Texture missing! Add a Splash Image.");

            return warnings.ToArray<string>();
        }
    }
}
