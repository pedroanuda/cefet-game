using Game.Gameplay;
using Game.Items;
using Godot;
using System;

namespace Game.UI
{
    public partial class SpellSlot : PanelContainer
    {
        [Signal]
        public delegate void PressedEventHandler(InputEventMouseButton @event, SpellSlot slot);

        [Signal]
        public delegate void StartDragEventHandler();

        [Signal]
        public delegate void StopDragEventHandler();

        private TextureRect _itemIcon;
        private Item _item;
        private CanvasItem _default_preview;
        private Drag _dragInstance;
        private bool _being_dragged = false;
        private bool _hovering = false;
        private bool _selected = false;

        public bool AllowDemoPreview { get; set; } = false;
        public bool Freezed { get; set; } = false;
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                ThemeTypeVariation = _selected
                    ? "SlotPanelSelected"
                    : "SlotPanel";
            }
        }

        [Export]
        public Item Item 
        {
            get => _item;
            set
            {
                _item = value;
                _itemIcon.Texture = _item?.Icon;
                _default_preview = new TextureRect
                {
                    Texture = Item?.Icon,
                    ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
                    Size = new Vector2(50, 50),
                    Position = GetLocalMousePosition()
                };
            }
        }

        private void PreviewHandler(CanvasItem p, InputEventMouseMotion e)
        {
            p.Set("position", p.GetParent() is Node2D
                ? p.GetParent<Node2D>().GetLocalMousePosition() 
                : e.Position
            );
            
            var i = Item as MathSpell;
            var hasComplexPreview = i.PreviewScene != null
                && i.PreviewScene._Bundled["names"].AsGodotArray().Contains(p.Name);
            

            if (AllowDemoPreview && i.PreviewScene != null && !hasComplexPreview)
                _dragInstance.SetPreview(i.PreviewScene.Instantiate<CanvasItem>(), 
                    GetNode("/root/MathWizards/Spells"));
            else if (hasComplexPreview && !AllowDemoPreview)
                _dragInstance.SetPreview(_default_preview.Duplicate() as CanvasItem,
                    GetNode("/root/MathWizards/UI"));
        }

        public override void _Ready()
        {
            _itemIcon = GetNode<TextureRect>("MarginContainer/IconRect");
            MouseEntered += () => _hovering = true;
            MouseExited += () => _hovering = false;

            _dragInstance = GetNode<Drag>("/root/MathWizards/UI/Drag");
            _itemIcon.Texture = Item?.Icon;
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseMotion me && me.Pressure == 1 && !_being_dragged)
            {
                _being_dragged = true;
                EmitSignal(SignalName.StartDrag);
                _dragInstance.Start(
                    data: Item,
                    preview: _default_preview.Duplicate() as CanvasItem,
                    customPreviewHandler: Item is MathSpell ? PreviewHandler : null,
                    parentNode: GetNode("/root/MathWizards/UI")
                );
            }

            if (@event is InputEventMouseButton mb)
            {
                if (!mb.Pressed) {
                    _being_dragged = false;
                    EmitSignal(SignalName.StopDrag);
                }

                if (Item is null || Freezed) return;
                if (mb.IsReleased() && mb.ButtonIndex == MouseButton.Left && _hovering)
                {
                    Selected = !Selected;
                    EmitSignal(SignalName.Pressed, mb, this);
                }
            }
        }
    }
}
