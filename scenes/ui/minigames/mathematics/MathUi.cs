using Godot;
using System.Collections.Generic;
using System;
using Game.Minigames;
using System.Linq;
using Game.Items;
using Game.Gameplay;
using Extensions;
using CefetGame.Ui;
using CefetGame.Misc;

namespace Game.UI
{
    public partial class MathUi : Control
    {
        // Questions and Answers
        private List<QuestionAndAnswers> _questions;
        private PanelContainer _answersBox;
        private GridContainer _answersContainer;
        private FlowContainer _questionsContainer;
        private AnimationPlayer _animationPlayer;
        private string _focusedQuestion;
        private int _questionsIndex = 0;

        // Spells
        private BoxContainer _spellsContainer;
        private Inventory _inventory;
        private SpellSlot[] _selectedSpells = new SpellSlot[2];
        private int _selectedSpellIndex = -1;
        private Area2D _playableArea;
        private Items.Item _draggedItem;
        private Drag _dragInstance;
        private SpellSlot _dragSlot;

        // Virtual Actions
        public Action<string> OnAnswerCorrect;
        public Func<Items.Item[], bool> CheckRecipe;
        public Action<Node, MathSpell> DropConclusion;

        // Helpers
        private MathParchment _parchment;

        [Export]
        private PackedScene _spellSlotScene;

        // Ui Focus
        private Label _switchingLabel;
        private SpellSlot lastSelectedSpell = null;
        private Control lastSelectedAnswer = null;
        private Control lastSelectedQuestion = null;
        private bool _questionsFocusMode = true;
        private bool _spellsFocusMode = false;
        private bool _answersFocusMode = false;
        private bool _dialogueOnScreen = false;
        private bool _spellCastingMode = false;

        public bool QuestionsFocusMode
        {
            get => _questionsFocusMode;
            set
            {
                if (_questionsFocusMode == value) return;

                _questionsFocusMode = value;
                foreach (var q in _questionsContainer.GetChildren())
                {
                    if (q is not Control cq) return;
                    cq.FocusMode = value 
                        ? FocusModeEnum.All 
                        : FocusModeEnum.None;
                }
            }
        }

        public bool SpellsFocusMode
        {
            get => _spellsFocusMode;
            set
            {
                if (_spellsFocusMode == value) return;
                _spellsFocusMode = value;

                foreach (var spellC in _spellsContainer.GetChildren())
                {
                    if (spellC is not SpellSlot c) break;
                    c.FocusMode = value && c.Item is not null
                        ? FocusModeEnum.All
                        : FocusModeEnum.None;
                }
            }
        }
        
        public bool AnswersFocusMode
        {
            get => _answersFocusMode;
            set
            {
                if (_answersFocusMode == value) return;
                _answersFocusMode = value;

                foreach (var ans in _answersContainer.GetChildren())
                {
                    if (ans is not Control c) break;
                    c.FocusMode = value
                        ? FocusModeEnum.All
                        : FocusModeEnum.None;
                }
            }
        }

        public Vector2 PreviewPosition { get => _dragInstance.PreviewPosition; }

        public override void _Ready()
        {
            _answersBox = GetNode<PanelContainer>("%Answers");
            _answersContainer = GetNode<GridContainer>("%AnswersContainer");
            _questionsContainer = GetNode<FlowContainer>("%QuestionsContainer");
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _spellsContainer = GetNode<BoxContainer>("%SpellsContainer");
            _dragInstance = GetNode<Drag>("../Drag");
            _parchment = GetNode<MathParchment>("Parchment");
            _switchingLabel = GetNode<Label>("%SwitchingLabel");

            GetNode<TextureButton>("%PauseButton").Pressed += () => GetNodeOrNull<PauseScreen>("../PauseScreen")?.HandlePause();
            foreach (var c in _questionsContainer.GetChildren()) c.QueueFree();
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionReleased("backy_back"))
            {
                if (!QuestionsFocusMode && !_parchment.Opened)
                {
                    AnswersFocusMode = false;
                    QuestionsFocusMode = true;
                    SpellsFocusMode = false;
                    _switchingLabel.Text = SpellsFocusMode ? "Quest\u00F5es" : "Feiti\u00e7os";

                    if (!_spellCastingMode)
                        HideAnswers();
                    else
                    {
                        _dragInstance.Stop();
                        _dragSlot?.StopDragging();

                        _spellCastingMode = false;
                    }

                    lastSelectedQuestion?.GrabFocus();
                    return;
                }
                OpenParchment();
            }
            else if (@event.IsActionReleased("switch_selection"))
            {
                if (AnswersFocusMode)
                    HideAnswers();

                AnswersFocusMode = false;
                QuestionsFocusMode = _spellCastingMode ? false : SpellsFocusMode;
                SpellsFocusMode = _spellCastingMode ? true : !SpellsFocusMode;
                _switchingLabel.Text = SpellsFocusMode ? "Quest\u00F5es" : "Feiti\u00e7os";

                if (_spellCastingMode) {
                    _dragInstance.Stop();
                    _dragSlot.StopDragging();

                    _spellCastingMode = false;
                }

                if (QuestionsFocusMode) {
                    lastSelectedQuestion?.GrabFocus();
                    DesselectSlots();
                }
                else if (SpellsFocusMode)
                    _spellsContainer.GetChild<Control>(0).GrabFocus();
            }
        }

        public override void _Input(InputEvent @event)
        {
            var isGamepad = GetNode<Global>("/root/Global").GamepadOn;
            _switchingLabel.GetParent<Control>().Visible = isGamepad;

            if (!isGamepad)
                return;

            if (@event.IsActionReleased("specialPress") && lastSelectedSpell.IsSpell)
            {
                DesselectSlots();
                _switchingLabel.Text = "Feiti\u00e7os";
                if (!_spellCastingMode) return;

                _dragInstance.OnDropAction?.Invoke(@event);
                _dragInstance.Stop();
                _dragSlot.StopDragging();

                _spellCastingMode = false;
                SpellsFocusMode = false;
                QuestionsFocusMode = true;
                AnswersFocusMode = false;

                lastSelectedQuestion.GrabFocus();
                _switchingLabel.Text = "Feiti\u00e7os";
            }
        }

        public void AddQuestions(List<QuestionAndAnswers> qAndAns)
        {
            _questions = qAndAns;

            for (int i = 0; i < qAndAns.Count; i++)
            {
                if (i == 6)
                    break;

                var button = GetQuestionButton(qAndAns[i]);
                _questionsContainer.AddChild(button);
                _questionsIndex++;

                if (i == 0) 
                    button.GrabFocus();
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
                slot.StartDrag += () => {
                    _draggedItem = slot.Item;
                    _dragSlot = slot;
                };
                slot.StopDrag += () => {
                    _draggedItem = null;
                    _dragSlot = null;
                };
                slot.FocusEntered += () => lastSelectedSpell = slot;
                slot.SpecialPressed += (e, slot) => {
                    _spellCastingMode = true;
                    SpellsFocusMode = false;
                };
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

            area.AreaEntered += Area =>
            {
                //GD.Print($"{Area.GetParent().Name}: {Area.GetParent().IsInGroup("preview")}");
                if (!Area.GetParent().IsInGroup("preview")) return;
                foreach (var slot in _spellsContainer.GetChildren())
                    (slot as SpellSlot).AllowDemoPreview = true;
                _dragInstance.OnDropAction = OnSucessfulDrop;
            };

            area.AreaExited += Area =>
            {
                if (!Area.GetParent().IsInGroup("preview")) return;
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

        public void OpenDialogue(string dialogueJsonPath, Action finishAction = null)
        {
            _dialogueOnScreen = true;
            var panel = GetNode<Panel>("Panel");
            panel.Visible = true;
            lastSelectedQuestion.ReleaseFocus();

            void onDialogueFinished()
            {
                finishAction?.Invoke();
                _dialogueOnScreen = false;
                DialogueSystem.Instance.DialogueFinished -= onDialogueFinished;
            }

            var dialogueUi = GetNode<NewDialogueUi>("NewDialogueUi");
            DialogueSystem.Instance.StartDialogue(dialogueUi, dialogueJsonPath);
            DialogueSystem.Instance.DialogueFinished += onDialogueFinished;
        }

        public void AddToParchment(Item item) =>
            _parchment.AddRecipesWith(item);

        public void OpenParchment()
        {
            if (_parchment.Opened)
                lastSelectedQuestion.GrabFocus();
            else
                lastSelectedQuestion?.ReleaseFocus();
            _parchment.Toggle();
        }

        public void OpenParchment(Item itemExpected)
        {
            lastSelectedQuestion?.ReleaseFocus();
            _parchment.Visible = true;
            _parchment.ShowRecipesWith(itemExpected);
        }

        public void TriggerGameOver()
        {
            GetNode<Control>("MainElements").Visible = false;
            GetNode<MathGameOverScreen>("GameOverScreen").TriggerUi();
        }

        public void SetRoundLabelText(string text)
        {
            GetNode<Label>("%RoundLabel").Text = text;
        }

        public void TriggerMainRoundLabel(string text)
        {
            GetNode<Label>("%MainRoundLabel").Text = text;

            GetNode<AnimationPlayer>("MainElements/RoundContainer/AnimationPlayer")
            .Play("default");
        }

        public void TriggerVictoryScreen(int score, int defeatedEnemies, int time, int totalScore)
        {
            GetNode<Control>("MainElements").Hide();
            var scr = GetNode<MathVictoryScreen>("VictoryScreen");
            scr.Score = score;
            scr.DefeatedEnemies = defeatedEnemies;
            scr.Time = time;
            scr.TotalScore = totalScore;

            scr.Show();
            scr.TriggerThing();
        }

        private void HideAnswers(Action callback = null)
        {
            void onAnimFinished(StringName animName)
            {
                _answersBox.Visible = false;
                _focusedQuestion = null;
                callback?.Invoke();

                _animationPlayer.AnimationFinished -= onAnimFinished;
            }
            _animationPlayer.AnimationFinished += onAnimFinished;
            _animationPlayer.Play("hide_answers");
        }

        private void ChangeAnswers(QuestionAndAnswers q)
        {
            async void onAnswerPressed(string answerText)
            {
                if (_animationPlayer.IsPlaying()) return;
                HideAnswers(() =>
                {
                    AnswersFocusMode = false;
                    QuestionsFocusMode = true;
                    if (!_dialogueOnScreen)
                        lastSelectedQuestion?.GrabFocus();
                });

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

                    var qIndex = question.GetIndex();
                    Callable.From(() => {
                        lastSelectedQuestion = _questionsContainer.GetChildOrNull<Control>(qIndex + 1);
                    }).CallDeferred();

                    if (_questionsIndex < _questions.Count - 1)
                        _questionsIndex++;
                    else
                    {
                        _questions.Shuffle();
                        _questionsIndex = 0;
                    }

                    _questionsContainer.AddChild(GetQuestionButton(_questions[_questionsIndex]));
                    return;
                }

                question.Disabled = true;
                await ToSignal(GetTree().CreateTimer(5f), SceneTreeTimer.SignalName.Timeout);
                question.Disabled = false;
            }

            // Adding the buttons and giving them their functionalities.
            var first = true;
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
                btn.FocusEntered += () =>
                {
                    lastSelectedAnswer = btn;
                    btn.Modulate = new Color("#bababa");
                };
                btn.FocusExited += () => btn.Modulate = new Color("#FFFFFF");

                _answersContainer.AddChild(btn);

                if (first)
                {
                    btn.GrabFocus();
                    first = false;
                }
            }
            AdjustAnimations();
        }

        private Button GetQuestionButton(QuestionAndAnswers qAndAns)
        {
            void onFinishTransition(StringName animName)
            {
                if (animName == "hide_answers")
                {
                    if (_focusedQuestion == qAndAns.question || GetNode<Global>("/root/Global").GamepadOn)
                    {
                        _answersBox.Visible = false;
                        _focusedQuestion = null;
                    } else {
                        _focusedQuestion = qAndAns.question;
                        ChangeAnswers(qAndAns);
                        _animationPlayer.Play("show_answers_fade_only");
                    }

                }
                _animationPlayer.AnimationFinished -= onFinishTransition;
            }

            void onQuestionButtonPressed()
            {
                _animationPlayer.AnimationFinished += onFinishTransition;

                if (!_answersBox.Visible)
                {
                    ChangeAnswers(qAndAns);
                    _answersBox.Visible = true;
                    _animationPlayer.Play("show_answers");
                    _focusedQuestion = qAndAns.question;
                    QuestionsFocusMode = false;
                    AnswersFocusMode = true;
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
                    "easy" => "GreenButton",
                    "medium" => "YellowButton",
                    "hard" => "RedButton",
                    _ => null
                }
            };
            btn.FocusEntered += () =>
            {
                lastSelectedQuestion = btn;
                btn.Modulate = new Color("#bababa");
            };
            btn.FocusExited += () => btn.Modulate = new Color("#FFFFFF");
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
                    slot.FocusMode = SpellsFocusMode ? FocusModeEnum.All : FocusModeEnum.None;
                } 
                else {
                    slot.Item = null;
                    slot.FocusMode = FocusModeEnum.None;
                }
            }
        }

        private void DesselectSlots() {
            if (_selectedSpellIndex >= 0) {
                for (int i = 0; i <= _selectedSpellIndex; i++) {
                    _selectedSpells[i].Selected = false;
                    _selectedSpells[i] = null;
                }
                _selectedSpellIndex = -1;
            }
        }

        private void SetSlotsFreezed(bool state)
        {
            foreach (var slot in _spellsContainer.GetChildren())
                if (slot is SpellSlot spSlot) spSlot.Freezed = state;
        }

        private async void OnSlotPressed(InputEvent @event, SpellSlot slot)
        {
            if (slot.Selected)
            {
                _selectedSpellIndex++;
                _selectedSpells[_selectedSpellIndex] = slot;
            } else {
                _selectedSpells[_selectedSpellIndex] = null;
                _selectedSpellIndex--;
            }

            if (_selectedSpellIndex < 1) 
                return;
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
            SetSlotsFreezed(false);

            // Checking if recipe was right. If so,
            // the code continues.
            var result = CheckRecipe?.Invoke(items);
            if (result != true) 
                return;

            // Gets the spells containers and give the correct one a focus;
            var containers = _spellsContainer.GetChildren();
             var lSPIdx = 0;
             if (lastSelectedSpell != null) {
                 lSPIdx = lastSelectedSpell.GetIndex();
             }

            if (lastSelectedSpell?.Item is null) {
                var previous = containers[lSPIdx != 0 ? lSPIdx - 1 : 0] as SpellSlot;
                var next = containers[lSPIdx != containers.Count - 1 ? lSPIdx + 1 : containers.Count - 1] as SpellSlot;

                if (previous.Item is not null)
                    previous.CallDeferred(Control.MethodName.GrabFocus);
                else if (next.Item is not null)
                    next.CallDeferred(Control.MethodName.GrabFocus);
            }
            else if (lastSelectedSpell.Item is not null)
                lastSelectedSpell.CallDeferred(Control.MethodName.GrabFocus);
        }

        private void OnSucessfulDrop(InputEvent e)
        {
            if (_draggedItem is not MathSpell ms || ms.SpellScenePath is null) return;

            var spell = GD.Load<PackedScene>(ms.SpellScenePath).Instantiate<Node>();
            if (spell is Node2D spell_2d)
            {
                if (e is InputEventMouseButton mb)
                    spell_2d.Position = mb.Position;
                else if (e is InputEventJoypadButton)
                    spell_2d.Position = _dragInstance.PreviewPosition;
            }
            DropConclusion?.Invoke(spell, _draggedItem as MathSpell);
        }

    }
}
