using Godot;
using System.Collections.Generic;
using System;
using Game.Minigames;
using System.Linq;
using Game.Items;
using Game.Gameplay;

namespace Game.UI
{
    public partial class MathUi : Control
    {
        // Questions and Answers
        private PanelContainer _answersBox;
        private GridContainer _answersContainer;
        private FlowContainer _questionsContainer;
        private AnimationPlayer _animationPlayer;
        private string _focusedQuestion;

        // Spells
        private BoxContainer _spellsContainer;
        private Inventory _inventory;
        private SpellSlot[] _selectedSpells = new SpellSlot[2];
        private int _selectedSpellIndex = -1;
        private Area2D _playableArea;
        private Items.Item _draggedItem;
        private Drag _dragInstance;

        // Virtual Actions
        public Action<string> OnAnswerCorrect;
        public Action<Items.Item[]> CheckRecipe;
        public Action<Node2D, MathSpell> DropConclusion;

        // Helpers
        private MathParchment _parchment;

        [Export]
        private PackedScene _spellSlotScene;

        public override void _Ready()
        {
            _answersBox = GetNode<PanelContainer>("%Answers");
            _answersContainer = GetNode<GridContainer>("%AnswersContainer");
            _questionsContainer = GetNode<FlowContainer>("%QuestionsContainer");
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _spellsContainer = GetNode<BoxContainer>("%SpellsContainer");
            _dragInstance = GetNode<Drag>("../Drag");
            _parchment = GetNode<MathParchment>("Parchment");

            GetNode<TextureButton>("%PauseButton").Pressed += () => GetNodeOrNull<PauseScreen>("../PauseScreen")?.HandlePause();
            foreach (var c in _questionsContainer.GetChildren()) c.QueueFree();
        }

        public void AddQuestions(List<QuestionAndAnswers> qAndAns)
        {
            foreach (var q in qAndAns)
            {
                _questionsContainer.AddChild(GetQuestionButton(q));
            }
            AdjustAnimations();
        }

        public void SyncInventory(Inventory inventory)
        {
            _inventory = inventory;
            _inventory.Updated += OnInventoryUpdate;

            for (int i = 0; i < _inventory.Capacity; i++)
            {
                var slot = _spellSlotScene.Instantiate<SpellSlot>();
                slot.Pressed += OnSlotPressed;
                slot.StartDrag += () => _draggedItem = slot.Item;
                slot.StopDrag += () => _draggedItem = null;
                _spellsContainer.AddChild(slot);
            }
        }

        public void SetPlayableArea(Area2D area)
        {
            area.MouseEntered += () =>
            {
                foreach (var slot in _spellsContainer.GetChildren())
                    (slot as SpellSlot).AllowDemoPreview = true;
                _dragInstance.OnDropAction = OnSucessfulDrop;
            };
            area.MouseExited += () =>
            {
                foreach (var slot in _spellsContainer.GetChildren())
                    (slot as SpellSlot).AllowDemoPreview = false;
                _dragInstance.OnDropAction = null;
            };
        }

        public void SyncHealthProgress(int maxHealth, int? newValue = null)
        {
            var healthBar = GetNode<TextureProgressBar>("%HealthProgressBar");
            healthBar.MaxValue = maxHealth;

            if (newValue is int n) healthBar.Value = n;
            else healthBar.Value = maxHealth;
        }

        public void UpdateHealthProgress(int health) =>
            GetNode<TextureProgressBar>("%HealthProgressBar").Value = health;

        public void OpenDialogue(DialogueCollection d, Action finishAction = null)
        {
            var panel = GetNode<Panel>("Panel");
            panel.Visible = true;
            GetNode<DialogueUi>("DialogueUi").Open(d, () =>
            {
                finishAction?.Invoke();
            });
        }

        public void AddToParchment(Item item) =>
            _parchment.AddRecipesWith(item);

        public void OpenParchment() =>
            _parchment.Toggle();

        public void OpenParchment(Item itemExpected)
        {
            _parchment.Visible = true;
            _parchment.ShowRecipesWith(itemExpected);
        }

        private void ChangeAnswers(QuestionAndAnswers q)
        {
            void hideAnswers(StringName s)
            {
                _answersBox.Visible = false;
                _animationPlayer.AnimationFinished -= hideAnswers;
            }

            async void onAnswerPressed(string answerText)
            {
                _animationPlayer.AnimationFinished += hideAnswers;
                _animationPlayer.Play("hide_answers");

                var question = _questionsContainer.GetChildren().ToList().Find(qP =>
                {
                    if (qP is Button qButton)
                        return qButton.Text == q.question;
                    else return false;
                }) as Button;

                // On correct answer.
                if (answerText == q.correctAnswer)
                {
                    OnAnswerCorrect?.Invoke(q.questionDifficulty);
                    question?.QueueFree();
                    return;
                }

                question.Disabled = true;
                await ToSignal(GetTree().CreateTimer(5f), SceneTreeTimer.SignalName.Timeout);
                question.Disabled = false;
            }

            // Adding the buttons and giving them their functionalities.
            foreach (var c in _answersContainer.GetChildren()) c.QueueFree();
            foreach (var a in q.answers)
            {
                var btn = new Button
                {
                    Text = a,
                    ThemeTypeVariation = "WhiteButton",
                    CustomMinimumSize = new Vector2(115, 0)
                };
                btn.Pressed += () => onAnswerPressed(btn.Text);

                _answersContainer.AddChild(btn);
            }
            AdjustAnimations();
        }

        private Button GetQuestionButton(QuestionAndAnswers qAndAns)
        {
            void onFinishTransition(StringName animName)
            {
                if (animName == "hide_answers")
                {
                    if (_focusedQuestion == qAndAns.question)
                    {
                        _answersBox.Visible = false;
                        _focusedQuestion = null;
                    } else {
                        _focusedQuestion = qAndAns.question;
                        ChangeAnswers(qAndAns);
                        _animationPlayer.Play("show_answers_fade_only");
                    }

                    _animationPlayer.AnimationFinished -= onFinishTransition;
                }
            }

            void onQuestionButtonPressed()
            {
                _animationPlayer.AnimationFinished += onFinishTransition;

                if (!_answersBox.Visible)
                {
                    _answersBox.Visible = true;
                    _animationPlayer.Play("show_answers");
                    // _focusedQuestion = qAndAns.question;
                    ChangeAnswers(qAndAns);
                    return;
                }

                _animationPlayer.Play("hide_answers");
            }

            var btn = new Button
            {
                Text = qAndAns.question,
                CustomMinimumSize = new Vector2(157, 0),
                ThemeTypeVariation = qAndAns.questionDifficulty switch
                {
                    "Easy" => "GreenButton",
                    "Medium" => "YellowButton",
                    "Hard" => "RedButton",
                    _ => null
                }
            };
            btn.Pressed += onQuestionButtonPressed;

            return btn;
        }

        private void AdjustAnimations()
        {
            var ansAndQ = _answersBox.GetParent<Control>();
            var anPlayer = _animationPlayer.GetAnimation("show_answers");
            var newXPosition = (ansAndQ.Size.X / 2) - (_answersBox.Size.X / 2);

            anPlayer?.TrackSetKeyValue(0, 0, new Vector2(
                newXPosition,
                ansAndQ.Size.Y - _answersBox.Size.Y
            ));

            anPlayer?.TrackSetKeyValue(0, 1, new Vector2(
                newXPosition,
                ansAndQ.Size.Y - _answersBox.Size.Y - _questionsContainer.Size.Y - 30
            ));
        }

        private void OnInventoryUpdate()
        {
            var inventoryItems = _inventory.GetItems();

            for (int i = 0; i < _inventory.Capacity; i++)
            {
                var slot = _spellsContainer.GetChild<SpellSlot>(i);

                if (inventoryItems.Length > i) 
                {
                    slot.Item = inventoryItems[i];
                } 
                else slot.Item = null;
            }
        }

        private void SetSlotsFreezed(bool state)
        {
            foreach (var slot in _spellsContainer.GetChildren())
                if (slot is SpellSlot spSlot) spSlot.Freezed = state;
        }

        private async void OnSlotPressed(InputEventMouseButton @event, SpellSlot slot)
        {
            if (slot.Selected)
            {
                _selectedSpellIndex++;
                _selectedSpells[_selectedSpellIndex] = slot;
            } else {
                _selectedSpells[_selectedSpellIndex] = null;
                _selectedSpellIndex--;
            }

            if (_selectedSpellIndex >= 1)
            {
                SetSlotsFreezed(true);
                await ToSignal(GetTree().CreateTimer(0.25f), SceneTreeTimer.SignalName.Timeout);

                Items.Item[] items = new Items.Item[2];
                for (int i = 0; i <= _selectedSpellIndex; i++)
                {
                    items[i] = _selectedSpells[i].Item;
                    _selectedSpells[i].Selected = false;
                    _selectedSpells[i] = null;
                }
                _selectedSpellIndex = -1;
                CheckRecipe?.Invoke(items);

                SetSlotsFreezed(false);
            }
        }

        private void OnSucessfulDrop(InputEventMouseButton e)
        {
            if (_draggedItem is not MathSpell ms || ms.SpellScenePath is null) return;
            var spell = GD.Load<PackedScene>((_draggedItem as MathSpell).SpellScenePath).Instantiate<Node2D>();
            spell.Position = e.Position;
            DropConclusion?.Invoke(spell, _draggedItem as MathSpell);
        }

    }
}
