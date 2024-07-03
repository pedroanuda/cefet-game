using Godot;
using System;

namespace Game
{
    public partial class Global : Node
    {
        private Node currentScene;

        public override void _Ready()
        {
            var root = GetTree().Root;
            currentScene = root.GetChild(root.GetChildCount() - 1);
        }
        public void GoToScene(string scenePath)
        {
            CallDeferred(MethodName.DefferedGoToScene, scenePath);
        }

        private void DefferedGoToScene(string scenePath)
        {
            currentScene.Free();

            var s = ResourceLoader.Load<PackedScene>(scenePath);
            currentScene = s.Instantiate();

            GetTree().Root.AddChild(currentScene);
            GetTree().CurrentScene = currentScene;
        }
    }
}
