using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Game
{
    public partial class Global : Node
    {
        private Node currentScene;
        private Action changeSceneAction;
        private Godot.Collections.Array<Godot.Collections.Dictionary> _leaderBoard;
        private bool _fullscreened = false;
        public bool GamepadOn { get; set; } = false;

        public void GoToScene(string scenePath, Action onChangedScene = null)
        {
            CallDeferred(MethodName.DefferedGoToScene, scenePath);
            changeSceneAction = onChangedScene;
        }

        public void TransitionToScene(string scenePath, string message = null, 
            float timeOnScreen = 0, 
            TransitionStyleEnum style = TransitionStyleEnum.SlideFade)
        {
            var handler = GetNode<TransitionHandler>("/root/TransitionHandler");

            // What happens after the first part of the transition.
            async void onHalf()
            {
                if (timeOnScreen > 0)
                    await ToSignal(GetTree().CreateTimer(timeOnScreen), SceneTreeTimer.SignalName.Timeout);
                       
                GoToScene(scenePath, onEnd);
                handler.TransitionInHalf -= onHalf;
            }

            // What happens when the scene is loaded.
            void onEnd()
            {
                handler.Play(style == TransitionStyleEnum.Circle
                   ? "change_to_minigame_end"
                   : "slide_end");
            }

            handler.TransitionInHalf += onHalf;
            handler.Play(style == TransitionStyleEnum.Circle
                ? "change_to_minigame_start"
                : "slide_start",
                message,
                timeOnScreen
            );
        }

        public override void _Ready()
        {
            var root = GetTree().Root;
            currentScene = root.GetChild(root.GetChildCount() - 1);

            
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionReleased("toggle_fullscreen"))
            {
                DisplayServer.WindowSetMode(_fullscreened
                    ? DisplayServer.WindowMode.Windowed
                    : DisplayServer.WindowMode.Fullscreen);

                _fullscreened = !_fullscreened;
                GetViewport().SetInputAsHandled();
            }
        }

        private void LoadLeaderboard() 
        {
            var fileLocation = "user://leaderboard.json";
            if (FileAccess.FileExists(fileLocation))
            {
                var f = FileAccess.GetFileAsString(fileLocation);
                _leaderBoard = Json.ParseString(f).As<Godot.Collections.Array<Godot.Collections.Dictionary>>();
            }
            else _leaderBoard = new();
        }

        private void SaveLeaderboard()
        {
            var fileLocation = "user://leaderboard.json";
            var f = FileAccess.Open(fileLocation, FileAccess.ModeFlags.Write);
            
        }

        private void DefferedGoToScene(string scenePath)
        {
            currentScene.Free();

            var s = ResourceLoader.Load<PackedScene>(scenePath);
            currentScene = s.Instantiate();

            GetTree().Root.AddChild(currentScene);
            GetTree().CurrentScene = currentScene;
            changeSceneAction?.Invoke();
            changeSceneAction = null;
        }
    }
}
