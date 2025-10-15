using Game.Gameplay;
using Godot;
using Godot.Collections;
using System;

namespace Game.Entities
{
	[Tool]
	public partial class Npc : CharacterBody2D
	{
		private Character _characterInfo;
		private AnimatedSprite2D _animatedSprite;
		private bool _hasSelect = false;
		public bool HasSelectState { get => _hasSelect; }
		private Vector2 _startPosition;
		private Vector2 _targetPosition;
		private bool _goingToTarget = true;
		private float _waitTimer = 0f;
		private bool _isWaiting = false;

		[Export]
		public Character CharacterInfo {
			get => _characterInfo;
			set
			{
				_animatedSprite ??= GetNodeOrNull<AnimatedSprite2D>("Sprites");

				if (_animatedSprite != null)
				{
					if (value is Character character)
					{
						_characterInfo = character;
						_animatedSprite.SpriteFrames = character.SpriteAnimations;
						_animatedSprite.Offset = character.SpritesOffset;
						_animatedSprite.Play();
						if (_animatedSprite.SpriteFrames is not null)
							_hasSelect = _animatedSprite.SpriteFrames.HasAnimation("idle_selected");
						else _hasSelect = false;
					}
					else
					{
						_characterInfo = null;
						if (_animatedSprite is null) return;
						_animatedSprite.SpriteFrames = null;
						_animatedSprite.Offset = Vector2.Zero;
						_hasSelect = false;
					}
				}else
				{
					_hasSelect = _characterInfo?.SpriteAnimations?.HasAnimation("idle_selected") ?? false;
				}
			}
		}

		[Export(PropertyHint.File, "*.json")]
		public string DialoguePath { get; set; }

		[Export]
		public Control InteractionUI { get; set; }

		[Export]
		public float PatrolDistanceX { get; set; } = 0f;

		[Export]
		public float PatrolDistanceY { get; set; } = 0f;

		[Export]
		public float PatrolSpeed { get; set; } = 40f;

		[Export]
		public float PatrolWaitTime { get; set; } = 2f;

		public bool IsInteracting { get; set; } = false;

		public void ToggleHighlight(bool state)
		{
			var animationName = _animatedSprite.Animation.ToString();
			var isCurrentAnimationHighlighted = animationName.EndsWith("selected");
			var frame = _animatedSprite.Frame;
			var frameProgress = _animatedSprite.FrameProgress;
			if (state && !isCurrentAnimationHighlighted)
			{
				_animatedSprite.Play($"{animationName}_selected");
				_animatedSprite.SetFrameAndProgress(frame, frameProgress);
			}
			else if (!state && isCurrentAnimationHighlighted)
			{
				_animatedSprite.Play(animationName.Replace("_selected", ""));
				_animatedSprite.SetFrameAndProgress(frame, frameProgress);
			}
		}

		public override void _PhysicsProcess(double delta)
		{

			if (IsInteracting)
			{
				Velocity = Vector2.Zero;
				MoveAndSlide();
				_animatedSprite.Stop();
				_animatedSprite.Play("idle");
				return; 
			}
			
			if (PatrolDistanceX == 0 && PatrolDistanceY == 0)
				return; 

			if (_isWaiting)
			{
				_waitTimer -= (float)delta;
				//henry peterson timer be like: "am i like, not delta or something?"
				UpdateAnimation(Vector2.Zero);

				if (_waitTimer <= 0f)
					_isWaiting = false;

				return;
			}


			Vector2 destination = _goingToTarget ? _targetPosition : _startPosition;
			Vector2 direction = (destination - GlobalPosition).Normalized();

			Velocity = direction * PatrolSpeed;
			MoveAndSlide();

			UpdateAnimation(direction);

			if (GlobalPosition.DistanceTo(destination) < 2f)
			{
				_goingToTarget = !_goingToTarget; 
				_isWaiting = true;
				_waitTimer = PatrolWaitTime;
				Velocity = Vector2.Zero;

			}
		}
		
		private void UpdateAnimation(Vector2 direction)
		{
			if (_animatedSprite == null) 
				return;

			string currentAnimation = _animatedSprite.Animation.ToString();
			string selected = "";
			
			if(currentAnimation.EndsWith("_selected"))
				selected = "_selected";

			if (direction.Length() < 0.1f)
			{
				_animatedSprite.Play("idle" + selected);
				return;
			}

			if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
			{
				if (direction.X > 0)
					_animatedSprite.Play("walk_right" + selected);
				else
					_animatedSprite.Play("walk_left" + selected);
			}
			else
			{
				if (direction.Y > 0)
					_animatedSprite.Play("walk_down" + selected);
				else
					_animatedSprite.Play("walk_up" + selected);
			}
		}

		public override void _Ready()
		{
			_animatedSprite = GetNode<AnimatedSprite2D>("Sprites");
			_startPosition = GlobalPosition;
			_targetPosition = _startPosition + new Vector2(PatrolDistanceX, PatrolDistanceY);
		}

		// public override void _Process(double delta)
		// {
		//     _animatedSprite ??= GetNode<AnimatedSprite2D>("Sprites");
		// }
	}
}
