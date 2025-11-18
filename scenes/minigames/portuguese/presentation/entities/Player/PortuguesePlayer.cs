using Godot;
using System;

public partial class PortuguesePlayer : CharacterBody2D
{
	[Export]
	public float Speed { get; set; } = 300.0f;

	public const float JumpVelocity = -400.0f;

	private AnimatedSprite2D sprites;

	public bool CanMove { get; set; } = true;
	public Vector2 Direction { get; private set; } = Vector2.Zero;
	public string lastDirection = "right";

	public override void _Ready()
	{
		sprites = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	public override void _Process(double delta)
	{
		if (Velocity.X > 0)
		{
			sprites.Play("walk_right");
			return;
		}
		else if (Velocity.X < 0)
		{
			sprites.Play("walk_left");
			return;
		}

		if (lastDirection == "right")
			sprites.Play("idle_right");
		else if (lastDirection == "left")
			sprites.Play("idle_left");
	}


	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Disabling jump for now
		// if (Input.IsActionJustPressed("ui_accept") && IsOnFloor() && CanMove)
		// {
		// 	velocity.Y = JumpVelocity;
		// }

		Direction = CanMove
			? Input.GetVector("left", "right", "up", "down")
			: Vector2.Zero;

		if (Direction != Vector2.Zero)
		{
			velocity.X = Direction.X * Speed;
			lastDirection = Direction.X > 0 ? "right" : "left";
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
