using Game.UI;
using Godot;
using System;

namespace Game.Gameplay
{
    [GlobalClass]
    public partial class LeaderboardInteractableArea : InteractableArea
    {
        private Player _playerInside;
        private LeaderboardPanel _leaderboardPanel;
        private bool _panelOpen = false;

        private Node2D _interactionCursor;
        private bool _isDebouncing = false;
        private Timer _debounceTimer;

        [Export]
        public string InteractionAction { get; set; } = "interact";

        public override void _Ready()
        {
            base._Ready();

            _leaderboardPanel = GetNode<GameplayUI>("/root/ClassroomScenario/UI")
                ?.GetNode<LeaderboardPanel>("LeaderboardPanel");
            if (_leaderboardPanel == null)
                GD.PrintErr("LeaderboardPanel não encontrado!");

            _debounceTimer = new Timer { OneShot = true, WaitTime = 0.12f };
            AddChild(_debounceTimer);
            _debounceTimer.Timeout += () => _isDebouncing = false;

            Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
            Connect("body_exited", new Callable(this, nameof(OnBodyExited)));
        }

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            if (_playerInside != null && Input.IsActionJustPressed(InteractionAction) && !_isDebouncing)
            {
                ToggleLeaderboard();
                StartDebounce();
            }
        }

        private void OnBodyEntered(Node body)
        {
            if (body is Player player)
            {
                _playerInside = player;
                ShowCursor(true);

                if (Input.IsActionPressed(InteractionAction) && !_isDebouncing)
                {
                    ToggleLeaderboard();
                    StartDebounce();
                }
            }
        }

        private void OnBodyExited(Node body)
        {
            if (body is Player player && _playerInside == player)
            {
                CloseLeaderboard();
                _playerInside = null;
                ShowCursor(false);
            }
        }

        private void ToggleLeaderboard()
        {
            if (_panelOpen)
                CloseLeaderboard();
            else if (_playerInside != null)
            {
                _leaderboardPanel.UpdateDisplay();
                _leaderboardPanel.Show();
                _playerInside.AllowActions = false;
                _panelOpen = true;
            }
        }

        private void CloseLeaderboard()
        {
            _leaderboardPanel?.Hide();
            if (_playerInside != null)
                _playerInside.AllowActions = true;
            _panelOpen = false;
        }

        private void StartDebounce()
        {
            _isDebouncing = true;
            _debounceTimer.Start();
        }

        private void ShowCursor(bool show)
        {
            if (_interactionCursor == null)
            {
                _interactionCursor = GD.Load<PackedScene>("res://scenes/entities/InteractionSelector.tscn")
                    .Instantiate<Node2D>();
                AddChild(_interactionCursor);
            }

            _interactionCursor.Visible = show;
            if (show)
                _interactionCursor.GlobalPosition = GlobalPosition;
        }

        public override void StartInteraction(Node2D interactor)
        {
            if (interactor is Player player)
            {
                _playerInside = player;
                ShowCursor(true);

                if (Input.IsActionPressed(InteractionAction) && !_isDebouncing)
                {
                    ToggleLeaderboard();
                    StartDebounce();
                }
            }
        }

        public override void StopInteraction(Node2D interactor)
        {
            if (interactor is Player player && _playerInside == player)
            {
                CloseLeaderboard();
                _playerInside = null;
                ShowCursor(false);
            }
        }
    }
}
