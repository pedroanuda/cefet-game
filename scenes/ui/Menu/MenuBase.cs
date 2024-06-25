using Game.UI;
using Godot;
using System;

public partial class MenuBase : Node2D
{
    private Control Buttons;

    public override void _Ready()
    {
        Buttons = GetNode<Control>("MenuButtons");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion) return;
        var isJoypad = @event is InputEventJoypadButton or InputEventJoypadMotion;

        if (isJoypad) Buttons.Visible = true;
        else Buttons.Visible = false;
    }
}
