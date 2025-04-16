using Game.Gameplay;
using Game.Items;
using Godot;
using System;
using System.Linq;

namespace Game.UI
{
    public partial class SpellSlot : PanelContainer
    {
        [Signal]
        public delegate void PressedEventHandler(InputEvent @event, SpellSlot slot);

        [Signal]
        public delegate void SpecialPressedEventHandler(InputEvent @event, SpellSlot slot);

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
        private bool _isSpell = false;

        public bool IsSpell { get => _isSpell; }
        public bool AllowDemoPreview { get; set; } = false;
        public bool Freezed { get; set; } = false;
        public bool Selected
        {
            get => _selected;
            set
            {
                if (_item is null) return;
                _selected = value;
                ThemeTypeVariation = _selected
                    ? "SlotPanelSelected"
                    : "SlotPanel";

                GetNode<Label>("%SelectLabel").Text = Selected
                    ? "Desselecionar"
                    : "Selecionar";
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

        public void StopDragging() {
            if (!_being_dragged) return;

            _being_dragged = false;
            EmitSignal(SignalName.StopDrag);
        }

        private void PreviewHandler(CanvasItem p, InputEvent e)
        {
            if (e is InputEventMouseMotion e1)
            {
                p.Set("position", p.GetParent() is Node2D
                    ? p.GetParent<Node2D>().GetLocalMousePosition()
                    : e1.Position
                );
            } 
            else if (e is InputEventJoypadButton e2)
            {
                var direction = Input.GetVector("left", "right", "up", "down");
                p.Set("position", (Vector2)p.Get("position") + direction * 10);
            }

            var i = Item as MathSpell;
            var hasComplexPreview = i.PreviewScene != null
                && i.PreviewScene._Bundled["names"].AsGodotArray().Contains(p.Name);
            

            if (AllowDemoPreview && i.PreviewScene != null && !hasComplexPreview)
                _dragInstance.SetPreview(i.PreviewScene.Instantiate<CanvasItem>(), 
                    _dragInstance.PreviewPosition, GetNode("/root/MathWizards/Battlefield"));
            else if (hasComplexPreview && !AllowDemoPreview)
                _dragInstance.SetPreview(_default_preview.Duplicate() as CanvasItem,
                    _dragInstance.PreviewPosition, GetNode("/root/MathWizards/UI"));
        }

        public override void _Ready()
        {
            _itemIcon = GetNode<TextureRect>("MarginContainer/IconRect");
            MouseEntered += () => _hovering = true;
            MouseExited += () => _hovering = false;

            _dragInstance = GetNode<Drag>("/root/MathWizards/UI/Drag");
            _itemIcon.Texture = Item?.Icon;

            var unfocused = GetThemeStylebox("panel").Duplicate() as StyleBox;
            var focused = GetThemeStylebox("panel").Duplicate() as StyleBox;
            focused.Set("bg_color", new Color("#545454"));

            FocusEntered += () =>
            {
                AddThemeStyleboxOverride("panel", focused);

                var index = GetIndex();
                var slotsWithItems = GetParent().GetChildren().Count(node => node is SpellSlot sl && sl.Item is not null);
                var arrows = GetNode<Node2D>("Arrows");
                _isSpell = Item.Id >= 103 && Item.Id <= 108;
                arrows.Show();
                arrows.GetNode<Sprite2D>("up").Visible = index > 0;
                arrows.GetNode<Sprite2D>("down").Visible = index < slotsWithItems - 1;

                GetNode<Label>("%ThrowLabel").GetParent<Control>().Visible = _isSpell;
                GetNode<Label>("%SelectLabel").GetParent<Control>().Visible = !_isSpell;
                GetNode<Control>("Helpers").Show();
            };

            FocusExited += () =>
            {
                RemoveThemeStyleboxOverride("panel");
                GetNode<Node2D>("Arrows").Hide();
                GetNode<Control>("Helpers").Hide();
            };
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
                    Press(mb);
            }

            if (@event is InputEventJoypadButton)
                if (@event.IsActionReleased("confirm"))
                    Press(@event);
                else if (@event.IsActionReleased("specialPress"))
                    SpecialPress(@event);
        }

        private void Press(InputEvent @event)
        {
            if (_item is null || _isSpell) return;
            Selected = !Selected;
            EmitSignal(SignalName.Pressed, @event, this);
        }

        private void SpecialPress(InputEvent @event)
        {
            if (_item is null || !_isSpell ) return;
            Selected = false;
            EmitSignal(SignalName.SpecialPressed, @event, this);

            if (_being_dragged)
            {
                _being_dragged = false;
                EmitSignal(SignalName.StopDrag);
            }
            else
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
        }
    }
}
