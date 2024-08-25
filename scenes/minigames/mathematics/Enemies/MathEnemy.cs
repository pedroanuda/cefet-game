using Godot;
using System;
using System.Collections.Generic;

namespace Game.Minigames
{
	[GlobalClass, Tool]
	public partial class MathEnemy : CharacterBody2D
	{
		static readonly Color HitColor = new("#ff816b");

		[Signal]
		public delegate void DiedEventHandler();

		[Export]
		public int Health 
		{ 
			get => _health;
			set
			{
				if (value > 0)
					_health = value;
				else _health = 0;
			}
		}

		[Export(PropertyHint.Range, "0,100")]
		public float Speed { get; set; }

		[Export]
		public int Damage { get; set; }

		[Export]
		public Vector2 Destination { get; set; }

		protected AnimatedSprite2D _enemySprite;
		protected NavigationAgent2D _navigationAgent;
		private int _health = 20;

		private Vector2 MovementTarget 
		{
			get => _navigationAgent is null ? Vector2.Zero : _navigationAgent.TargetPosition;
			set {
				if (_navigationAgent is not null)
					_navigationAgent.TargetPosition = value;
			}
		}

		public virtual void TakeDamage(int amount) 
		{ 
			Health -= amount;
			PlayHitAnimation();

			if (Health <= 0)
				Die();
		}

		public void Die()
		{
            EmitSignal(SignalName.Died);
			_navigationAgent.Velocity = Vector2.Zero;

			PlayDeathAnimation(() => QueueFree());
			var smoke = GetNodeOrNull<CpuParticles2D>("DeathSmoke");
			if (smoke is not null)
			{
				smoke.Emitting = true;
				smoke.Finished += smoke.QueueFree;
				smoke.Reparent(GetParent());
			}
        }

		public override void _Ready()
		{
			_enemySprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");
			_navigationAgent = GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");

			if (!Engine.IsEditorHint()) Callable.From(ActorSetup).CallDeferred();
		}

		public override void _Process(double delta)
		{
			if (!Engine.IsEditorHint()) return;

			_enemySprite ??= GetNodeOrNull<AnimatedSprite2D>("Sprite");
			_navigationAgent ??= GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");
		}

		public override void _PhysicsProcess(double delta)
		{
			if (_navigationAgent.IsNavigationFinished() || Engine.IsEditorHint()) return;


			var currentPosition = GlobalTransform.Origin;
			var nextPosition = _navigationAgent.GetNextPathPosition();

			Velocity = currentPosition.DirectionTo(nextPosition) * Speed;
			MoveAndSlide();

		}

		public override string[] _GetConfigurationWarnings()
		{
			var warnings = new List<string>();
			if (_enemySprite is null) warnings.Add("Sprite node \"Sprite\" is missing.");
			if (_navigationAgent is null) warnings.Add("Navigation Agent missing. Create a \"NavigationAgent2D\" node.");

			return warnings.ToArray();
		}

		private async void ActorSetup()
		{
			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
			MovementTarget = Destination;
		}

		private void PlayHitAnimation(Action afterAnimation = null)
		{
			var anPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			void onEnd(StringName animName)
			{
				afterAnimation?.Invoke();
				anPlayer.AnimationFinished -= onEnd;
			}

			if (!anPlayer.HasAnimation("hit"))
			{
				var hitAnim = new Animation();
				var trackIdx = hitAnim.AddTrack(Animation.TrackType.Value);

				hitAnim.TrackSetPath(trackIdx, ".:modulate");
				hitAnim.TrackInsertKey(trackIdx, 0, Modulate);
				hitAnim.TrackInsertKey(trackIdx, .25f, HitColor);
				hitAnim.TrackInsertKey(trackIdx, .5f, new Color("#ffffff"));

				anPlayer.GetAnimationLibrary("").AddAnimation("hit", hitAnim);
				anPlayer.Play("hit");
				return;
			}

			var anim = anPlayer.GetAnimation("hit");
			var track = anim.FindTrack(".:modulate", Animation.TrackType.Value);
			anim.TrackSetKeyValue(track, 0, Modulate);
			anim.TrackSetKeyValue(track, 1, HitColor);
			anim.TrackSetKeyValue(track, 2, new Color("#ffffff"));

			if (afterAnimation is not null) anPlayer.AnimationFinished += onEnd;
			anPlayer.Play("hit");
		}

		private void PlayDeathAnimation(Action onEnd)
		{
			var anPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			anPlayer.AnimationFinished += (StringName animName) => onEnd();
			if (anPlayer.HasAnimation("death"))
			{
				anPlayer.Play("death");
				return;
			}

			var animation = new Animation();
			var track = animation.AddTrack(Animation.TrackType.Value);

			animation.TrackSetPath(track, ".:modulate");
			animation.TrackInsertKey(track, 0, Modulate);
			animation.TrackInsertKey(track, .10f, HitColor);

			anPlayer.GetAnimationLibrary("").AddAnimation("death", animation);
			anPlayer.Play("death");
		}
	}
}
