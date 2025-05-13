using CefetGame.Gameplay;
using Godot;
using Newtonsoft.Json;
using System.Timers;

namespace CefetGame.Ui
{
    public partial class NewDialogueUi : Control
    {
        [Signal]
        public delegate void OptionChosenEventHandler(string choiceJson);

        [Signal]
        public delegate void AdvancedEventHandler();

        [Export(PropertyHint.File, "*.tscn,*.scn")]
        public string OptionButtonPath { get; set; }
        protected PackedScene OptionButtonScene { get; set; }
        protected RichTextLabel TitleNode { get; set; }
        protected RichTextLabel BodyNode { get; set; }
        protected BoxContainer OptionsContainer { get; set; }
        protected System.Timers.Timer CharAnimationTimer { get; set; }
        protected Godot.Timer DelayTimer { get; set; }
        protected bool IsChoiceDialogue { get; set; } = false;
        protected bool IsDialogueFullyVisible { get; set; } = false;

        protected int CurrentVisibleCharacters { 
            get => _currentVisibleCharacters;
            set 
            {
                _currentVisibleCharacters = value;
                BodyNode.VisibleCharacters = value;
            }
        }
        protected float SecondsPerChar { get; set; } = 0.05f;

        private int _currentVisibleCharacters = 0;

        public virtual void StartDialogue(DialoguePart dialoguePart)
        {
            Show();
            ChangeDialogue(dialoguePart);
        }

        public virtual void ChangeDialogue(DialoguePart dialoguePart)
        {
            CurrentVisibleCharacters = 0;
            TitleNode.Text = $"[b]{dialoguePart.Title}[/b]";
            BodyNode.Text = dialoguePart.Body;
            IsChoiceDialogue = dialoguePart.Choices is not null;
            if (IsChoiceDialogue)
            {
                OptionsContainer.Show();
                PrepareOptionsButtons(dialoguePart.Choices);
            }
            else OptionsContainer.Visible = false;

            CharAnimationTimer.Interval = 50;
            CharAnimationTimer.Start();
        }

        public virtual void Finish()
        {
            Hide();
            CurrentVisibleCharacters = 0;
        }

        protected void TryAdvance()
        {
            if (CurrentVisibleCharacters < BodyNode.Text.Length)
            {
                CharAnimationTimer.Stop();
                CurrentVisibleCharacters = BodyNode.Text.Length;
                IsDialogueFullyVisible = true;
                if (IsChoiceDialogue)
                    EnableOptions(0.5); //EnableOptions potencialmente acontecendo mais de uma vez por estar em uma thread diferente
                return;
            }

            if (!IsChoiceDialogue)
                EmitSignal(SignalName.Advanced);
        }

        protected void PrepareOptionsButtons(DialogueChoice[] choices)
        {
            foreach (DialogueChoice choice in choices)
            {
                var option = OptionButtonScene.Instantiate<Button>();
                option.Text = choice.Text;
                option.Pressed += () => OnOptionPressed(choice);
                option.SizeFlagsVertical = SizeFlags.ExpandFill;
                option.Disabled = true;
                OptionsContainer.AddChild(option);
            }
        }

        protected void EnableOptions()
        {
            if (!IsChoiceDialogue) return;

            foreach (var node in OptionsContainer.GetChildren())
            {
                if (node is Button option)
                    option.Disabled = false;
            }
            OptionsContainer.GetChild<Button>(0).GrabFocus();
        }

        protected void EnableOptions(double delay)
        {
            void onDelayTimeout()
            {
                EnableOptions();
                DelayTimer.Timeout -= onDelayTimeout;
            };

            DelayTimer.OneShot = true;
            if (!DelayTimer.IsConnected(Godot.Timer.SignalName.Timeout, Callable.From(onDelayTimeout)))
            {
                DelayTimer.Timeout += onDelayTimeout;
            }
            DelayTimer.Start(delay);
        }

        protected void OnOptionPressed(DialogueChoice choice)
        {
            var choiceJson = JsonConvert.SerializeObject(choice);
            EmitSignal(SignalName.OptionChosen, choiceJson);

            foreach (var child in OptionsContainer.GetChildren())
            {
                child.QueueFree();
            }
        }

        private void OnCharacterAnimationTimeout()
        {
            CurrentVisibleCharacters += 1;
            IsDialogueFullyVisible = false;

            if (CurrentVisibleCharacters >= BodyNode.Text.Length)
            {
                CharAnimationTimer.Stop();
                IsDialogueFullyVisible = true;
                if (IsChoiceDialogue)
                    EnableOptions(0.5);
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey key && key.IsReleased())
            {
                if (key.Keycode == Key.Enter || key.Keycode == Key.Space)
                {
                    TryAdvance();

                    if (!IsChoiceDialogue)
                    GetViewport().SetInputAsHandled();
                }
            }
        }

        public override void _Ready()
        {
            TitleNode = GetNode<RichTextLabel>("%DialogueTitle");
            BodyNode = GetNode<RichTextLabel>("%DialogueBody");
            OptionsContainer = GetNode<BoxContainer>("%OptionsContainer");
            DelayTimer = GetNode<Godot.Timer>("DelayTimer");
            OptionButtonScene = ResourceLoader.Load<PackedScene>(OptionButtonPath);
            BodyNode.VisibleCharacters = 0;

            // CharAnimation Timer configuration
            void onTimerElapsed(object source, ElapsedEventArgs e)
            {
                CallDeferred(MethodName.OnCharacterAnimationTimeout);
            }
            CharAnimationTimer = new System.Timers.Timer
            {
                Enabled = false,
                AutoReset = true,
                Interval = SecondsPerChar * 1000
            };
            CharAnimationTimer.Elapsed += new ElapsedEventHandler(onTimerElapsed);
        }
    }
}
