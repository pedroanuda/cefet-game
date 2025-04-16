using Game;
using Game.Items;
using Godot;
using System;

namespace Game.Gameplay
{
	public partial class Player : CharacterBody2D
	{
		private AnimationTree anim_tree;
		private Vector2 direction = Vector2.Zero;
		private string _backpack = "None";

		public bool AllowActions { get; set; } = true;
		[Export] public float Speed = 300.0f;
		public Inventory Inventory { get; } = new();
		public Stats Stats { get; } = new();

		[Export(PropertyHint.Enum, "None,Simple")]
		public string Backpack { 
			get => _backpack;
			set {
				_backpack = value;
				UpdateBackpack(value);
			}
		}

		[Export]
		private Godot.Collections.Array<Item> _initialItems;

		public override void _Ready()
		{
			anim_tree = GetNode<AnimationTree>("AnimationTree");
			anim_tree.Active = true;
		
			UpdateBackpack(Backpack);
			foreach (Item item in _initialItems) Inventory.Add(item);
		}

		public override void _Process(double delta)
		{
			UpdateMovements();
		}

		public override void _PhysicsProcess(double delta)
		{
			Vector2 velocity = Velocity;

			// Get the input direction and handle the movement/deceleration.
			direction = AllowActions
				? Input.GetVector("left", "right", "up", "down").Normalized()
				: Vector2.Zero;

			if (direction != Vector2.Zero)
			{
				velocity.X = direction.X * Speed;
				velocity.Y = direction.Y * Speed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
				velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
			}

			Velocity = velocity;
			MoveAndSlide();
		}

		private void UpdateMovements()
		{
			if (Velocity == Vector2.Zero) 
			{
				anim_tree.Set("parameters/conditions/IsIdle", true);
				anim_tree.Set("parameters/conditions/IsMoving", false);
			}
			else
			{
				anim_tree.Set("parameters/conditions/IsIdle", false);
				anim_tree.Set("parameters/conditions/IsMoving", true);
			}

			if (direction != Vector2.Zero)
			{
				anim_tree.Set("parameters/Idle/blend_position", direction);
				anim_tree.Set("parameters/Walk/blend_position", direction);
			}
		}

		private void UpdateBackpack(string backpackName)
		{
			GetNode<Node2D>("Backpacks").Visible = true;

			switch (backpackName)
			{
				case "Simple":
					GetNode<Sprite2D>("%SimpleBackpack").Visible = true;
					break;
				case "None":
				default:
					GetNode<Node2D>("Backpacks").Visible = false;

					break;
			}
		}
	}
}
