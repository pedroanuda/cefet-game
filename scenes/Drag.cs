using Godot;
using System;

namespace Game.Gameplay
{
    [GlobalClass]
    public partial class Drag : Node
    {
        private Variant _data;
        private CanvasItem _preview;
        private Node _parent;
        private Action<CanvasItem, InputEventMouseMotion> _previewHandler;
        public Action<InputEventMouseButton> OnDropAction;

        private void AdjustPreview()
        {
            _preview.Set("position", GetViewport().GetMousePosition());

            if (_preview is Control ctrl)
                ctrl.MouseFilter = Control.MouseFilterEnum.Ignore;

            if (_parent is not null) _parent.AddChild(_preview);
            else GetTree().Root.AddChild(_preview);
        }

        public void SetPreview(CanvasItem newPreview, Node newParent = null)
        {
            _preview.Free();
            _preview = newPreview;
            _parent = newParent ?? _parent;
            AdjustPreview();
        }

        public void Start(Variant data, CanvasItem preview,
            Action<CanvasItem, InputEventMouseMotion> customPreviewHandler = null,
            Node parentNode = null
        )
        {
            _data = data;
            _preview = preview;
            _previewHandler = customPreviewHandler;
            
            _parent = parentNode;
            AdjustPreview();
        }

        public void Stop()
        {
            _data = new Variant();
            _preview.QueueFree();
            _preview = null;
        }

        public override void _Input(InputEvent @event)
        {
            if (_preview is null) return;
            if (@event is InputEventMouseButton e1 && e1.ButtonIndex == MouseButton.Left && e1.IsReleased())
            {
                OnDropAction?.Invoke(e1);
                Stop();
            }
            if (@event is InputEventMouseMotion e)
            {
                if (_previewHandler is not null) _previewHandler.Invoke(_preview, e);
                else _preview.Set("position", _parent is Node2D p ? p.ToLocal(e.GlobalPosition) : e.Position);
            }   
        }
    }
}
