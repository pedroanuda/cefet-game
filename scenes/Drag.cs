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
        private Action<CanvasItem, InputEvent> _previewHandler;
        public Action<InputEvent> OnDropAction;

        public Vector2 PreviewPosition {
            get => (Vector2) _preview.Get("position");
        }

        private void AdjustPreview()
        {
            if (!GetNode<Global>("/root/Global").GamepadOn)
                _preview.Set("position", GetViewport().GetMousePosition());
            else
                _preview.Set("position", new Vector2(500, 300));

            if (_preview is Control ctrl)
                ctrl.MouseFilter = Control.MouseFilterEnum.Ignore;

            if (_parent is not null) _parent.AddChild(_preview);
            else GetTree().Root.AddChild(_preview);
        }

        public void SetPreview(CanvasItem newPreview, Node newParent = null)
        {
            var area = _preview.GetNode("PreviewArea");
            _preview.RemoveChild(area);
            _preview.Free();
            _preview = newPreview;
            _preview.AddToGroup("preview");
            _parent = newParent ?? _parent;
            _preview.AddChild(area);
            area.Set(Node2D.PropertyName.GlobalScale, Vector2.One);
            AdjustPreview();
        }

        public void SetPreview(CanvasItem newPreview, Vector2 newPosition, Node newParent = null) {
            SetPreview(newPreview, newParent);
            newPreview.Set("position", newPreview.IsInGroup("freez_preview")
                ? new Vector2(newPosition.X, 520)
                : newPosition);
        }

        public void Start(Variant data, CanvasItem preview,
            Action<CanvasItem, InputEvent> customPreviewHandler = null,
            Node parentNode = null
        )
        {
            _data = data;
            _preview = preview;
            _preview.AddToGroup("preview");

            var area = ResourceLoader.Load<PackedScene>("res://scenes/ui/minigames/mathematics/SpellSlot/PreviewArea.tscn").Instantiate();
            _preview.AddChild(area);
            _previewHandler = customPreviewHandler;
            
            _parent = parentNode;
            AdjustPreview();
        }

        public void Stop()
        {
            _data = new Variant();
            _preview?.QueueFree();
            _preview = null;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!GetNode<Global>("/root/Global").GamepadOn || _preview is null) return;

            var direction = Input.GetVector("left", "right", "up", "down");
            var previousPosition = (Vector2) _preview.Get("position");

            if (_previewHandler is not null) _previewHandler.Invoke(_preview, new InputEventJoypadButton());
            else _preview.Set("position", _preview.IsInGroup("freez_preview")
                ? new Vector2(previousPosition.X, 520)
                : previousPosition + direction * 10);
        }

        public override void _Input(InputEvent @event)
        {
            if (_preview is null) return;

            if (!GetNode<Global>("/root/Global").GamepadOn)
            {
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
                return;
            }
        }
    }
}
