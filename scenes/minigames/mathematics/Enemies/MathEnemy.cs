using Godot;
using System;
using System.Collections.Generic;


namespace Game.Minigames
{
	public interface ISlowable {
		public float SlowPercentage {get; set;}
		void Slow(double percentage, double duration, bool fromOriginalSpeed);
	}

	[GlobalClass, Tool]
	public partial class MathEnemy : CharacterBody2D, ISlowable
	{
		public static readonly Color HitColor = new("#ff816b");
		static readonly Color FrozenColor = new("#1297ff");

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
		private AnimationPlayer _animPlayer;
		private int _health = 20;
		private bool dead;

		// Slowing variables.
		public bool Frozen { get; set; } = false;
		public float SlowPercentage { get; set; } = 0;
		private float _original_speed;
		private Timer _slow_timer;

		public bool Dead { get => dead; }

		private Vector2 MovementTarget 
		{
			get => _navigationAgent is null ? Vector2.Zero : _navigationAgent.TargetPosition;
			set {
				if (_navigationAgent is not null)
					_navigationAgent.TargetPosition = value;
			}
		}

		public virtual void TakeDamage(int amount, bool animationOn = true) 
		{
			if (Health <= 0) return;
			Health -= amount;
			if (animationOn) PlayHitAnimation();

			if (Health <= 0)
				Die();
		}

		public void Die()
		{
            EmitSignal(SignalName.Died);
			GetNode<AnimatedSprite2D>("Sprite").Stop();
			dead = true;

			PlayDeathAnimation(() => QueueFree());
			var smoke = GetNodeOrNull<CpuParticles2D>("DeathSmoke");
			if (smoke is not null)
			{
				smoke.Emitting = true;
				smoke.Finished += smoke.QueueFree;
				smoke.Reparent(GetParent());
			}
        }

		public void Slow(double percentage, double duration, bool fromOriginalSpeed = true) {
			if (Frozen || Dead) return;
			Speed = fromOriginalSpeed 
			? _original_speed - (_original_speed * (float) percentage)
			: Speed - (Speed * (float) percentage);

			SlowPercentage = (float) percentage;

			if (duration > _slow_timer.TimeLeft)
				_slow_timer.Start(duration);
			else
				_slow_timer.Start();
		}

		public void Freeze(double duration)
		{
			var timer = GetNode<Timer>("FreezeTimer");
			var sprite = GetNode<AnimatedSprite2D>("Sprite");
			Speed = 0;
			sprite.Stop();
			Frozen = true;

			if (duration > timer.WaitTime)
				timer.WaitTime = duration;

			Modulate = FrozenColor;
			timer.Start();
		}

		public void Unfreeze()
		{

		}

		public override void _Ready()
		{
			_enemySprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");
			_navigationAgent = GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");
			_slow_timer = GetNodeOrNull<Timer>("SlowTimer");
			_original_speed = Speed;
			_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

			_slow_timer.Timeout += OnSlowTimerOut;

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
			if (_navigationAgent.IsNavigationFinished() || Engine.IsEditorHint() || dead) return;

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

		private void OnSlowTimerOut() {
			Speed = _original_speed;
			SlowPercentage = 0;
		}

		private void OnFreezeTimerOut()
		{
			GetNode<AnimatedSprite2D>("Sprite").Play();
			Frozen = false;
			Slow(0.5, 5);

			// Animation Handling
			if (_animPlayer.HasAnimation("unfreeze"))
			{
				var anim = _animPlayer.GetAnimation("unfreeze");
				var trIdx = anim.FindTrack(".:modulate", Animation.TrackType.Value);

				anim.TrackSetKeyValue(trIdx, 0, FrozenColor);
				_animPlayer.Play("unfreeze");
			}
		}

		private void PlayHitAnimation(Action afterAnimation = null)
		{
			void onEnd(StringName animName)
			{
				afterAnimation?.Invoke();
				_animPlayer.AnimationFinished -= onEnd;
			}

			if (!_animPlayer.HasAnimation("hit"))
			{
				var hitAnim = new Animation();
				var trackIdx = hitAnim.AddTrack(Animation.TrackType.Value);

				hitAnim.TrackSetPath(trackIdx, ".:modulate");
				hitAnim.TrackInsertKey(trackIdx, 0, Modulate);
				hitAnim.TrackInsertKey(trackIdx, .25f, HitColor);
				hitAnim.TrackInsertKey(trackIdx, .5f, new Color("#ffffff"));

				_animPlayer.GetAnimationLibrary("").AddAnimation("hit", hitAnim);
				_animPlayer.Play("hit");
				return;
			}

			var anim = _animPlayer.GetAnimation("hit");
			var track = anim.FindTrack(".:modulate", Animation.TrackType.Value);
			anim.TrackSetKeyValue(track, 0, Modulate);
			anim.TrackSetKeyValue(track, 1, HitColor);
			anim.TrackSetKeyValue(track, 2, new Color("#ffffff"));

			if (afterAnimation is not null) _animPlayer.AnimationFinished += onEnd;
			_animPlayer.Play("hit");
		}

		private void PlayDeathAnimation(Action onEnd)
		{
			_animPlayer.AnimationFinished += (StringName animName) => onEnd();
			if (_animPlayer.HasAnimation("death"))
			{
				_animPlayer.Play("death");
				return;
			}

			var animation = new Animation();
			var track = animation.AddTrack(Animation.TrackType.Value);

			animation.TrackSetPath(track, ".:modulate");
			animation.TrackInsertKey(track, 0, Modulate);
			animation.TrackInsertKey(track, .10f, HitColor);

			_animPlayer.GetAnimationLibrary("").AddAnimation("death", animation);
			_animPlayer.Play("death");
		}
	}
}
