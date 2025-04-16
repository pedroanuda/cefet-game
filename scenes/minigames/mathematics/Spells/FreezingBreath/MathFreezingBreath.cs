using Godot;
using System;

namespace Game.Minigames
{
    public partial class MathFreezingBreath : Node2D
    {
        [Export(PropertyHint.File, "*.tscn,*.scn")]
        private string FrezzedPathScene { get; set; }

        [Export]
        public double Speed { get; set; } = 5;

        [Export]
        public double Limit { get; set; } = -100;

        [Export]
        public Vector2 Start 
        { 
            get => _start;
            set {
                _start = value;
                GlobalPosition = _start;
            } 
        }

        private Sprite2D _pathMask;
        private Sprite2D _breathSprite;
        private Vector2 _start;
        private PackedScene _freezedPath;
        private float _initialYScale = 0;
        private float _counter = 0;

        public override void _Ready()
        {
            _breathSprite = GetNode<Sprite2D>("Breath");
            GlobalPosition = Start;
            _freezedPath = ResourceLoader.Load<PackedScene>(FrezzedPathScene);
            _pathMask = GetNode<Sprite2D>("Path");
            _initialYScale = _pathMask.GlobalScale.Y;

            var area = GetNode<Area2D>("Breath/FreezingBreathArea");
            area.BodyEntered += OnBodyEnteredArea;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_breathSprite.GlobalPosition.Y <= Limit)
            {
                var storage = _pathMask.GetNode<Node2D>("Storage");
                var lastChild = storage.GetChild(-1);

                if (lastChild.HasSignal("melted") && !lastChild.IsConnected("melted", Callable.From(QueueFree)))
                {
                    lastChild.Connect("melted", Callable.From(QueueFree));
                    _breathSprite.Hide();
                    _breathSprite.GetNode<Area2D>("FreezingBreathArea").Monitoring = false;
                }
                return;
            }

            var velocity = (float) Speed * 10 * (float) delta;
            _counter += velocity;

            if (_counter >= 28) // Valor de px que passam para acrescentar um novo freezedPath;
            {
                _counter -= 28;

                var instance = _freezedPath.Instantiate<Node2D>();
                GetNode<Node2D>("Path/Storage").AddChild(instance);
                instance.GlobalPosition = new Vector2 {
                    X = _breathSprite.GlobalPosition.X,
                    Y = _breathSprite.GlobalPosition.Y + _counter
                };
            }

            _breathSprite.Position = new Vector2
            {
                X = _breathSprite.Position.X,
                Y = _breathSprite.Position.Y - velocity
            };

            _pathMask.Scale = new Vector2 
            {
                X = _pathMask.Scale.X,
                Y = _pathMask.Scale.Y + velocity
            };
            GetNode<Node2D>("Path/Storage").GlobalScale = Vector2.One;
            // new Vector2 (
            //     _pathMask.GlobalScale.X,
            //     _initialYScale / _pathMask.GlobalScale.Y
            // );
        }

        private void OnBodyEnteredArea(Node2D body)
        {
            if (body is not MathEnemy enemy) return;
            enemy.Freeze(10);
        }
    }
}
