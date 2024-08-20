using System.Collections.Generic;
using System;
using Godot;
using System.Linq;
using Game.Gameplay;

namespace Game.Minigames
{
    public struct QuestionAndAnswers
    {
        public string question;
        public string correctAnswer;
        public string questionDifficulty;
        public string[] answers;
    }

    public partial class MathWizards : Node2D
    {
        private UI.MathUi Ui;
        private Inventory _inventory = new(6);
        private int _health = 100;

        // Base Items
        [Export] public int WizardHealth 
        { 
            get => _health;
            set {
                if (value > 0)
                    _health = value;
                else _health = 0;
                Ui?.UpdateHealthProgress(_health);
            }
        }
        [Export] public Items.Item EasyAnswerItem { get; set; }
        [Export] public Items.Item MediumAnswerItem { get; set; }
        [Export] public Items.Item HardAnswerItem { get; set; }

        // Spells
        [ExportCategory("Spells")]
        [Export] public Node2D WizardNode { get; set; }
        [Export] public Godot.Collections.Array<Items.MathSpell> PossibleSpells { get; set; }
        [ExportGroup("Debugging")]
        [Export] public bool UnlimitedSpells { get; set; }

        // Enemies Handling
        private readonly List<MathEnemy> _enemiesUnderWall = new();
        [ExportCategory("Enemies")]
        [Export] public Godot.Collections.Array<PackedScene> EnemiesScenes;
        [Export] public Vector2 GoalDestiny { get; set; }
        [ExportGroup("Spawner", "Spawner")]
        [Export] public float SpawnerYOrigin { get; set; }
        [Export] public float SpawnerXStart { get; set; }
        [Export] public float SpawnerXEnd { get; set; }

        // Helpers Logic
        [ExportCategory("Helpers")]
        [Export] public DialogueCollection FirstParchmentDialogue { get; set; }
        [ExportGroup("Helpers Debugging")]
        [Export] private bool FirstTimePlaying { get; set; } = true;
        private int answersAnsweredCorrectly;

        public override void _Ready()
        {
            Ui = GetNode<UI.MathUi>("UI/MathUi");

            var questions = new List<QuestionAndAnswers>
            {
                new() {
                    question = "10 + 5 = ?",
                    answers = new string[] {"15", "35", "20", "10"},
                    correctAnswer = "15",
                    questionDifficulty = "Easy"
                },
                new() {
                    question = "28 \u00F7 7 = ?",
                    answers = new string[] {"7", "9", "1", "4"},
                    correctAnswer = "4",
                    questionDifficulty = "Medium"
                },
                new() {
                    question = "2x + 10 = 30",
                    answers = new string[] {"x = 15", "x = 8", "x = 10", "x = 6"},
                    correctAnswer = "x = 10",
                    questionDifficulty = "Hard"
                },
                new()
                {
                    question = "3x - 5 = 10",
                    answers = new string[] {"x = 10", "x = 2", "x = 7", "x = 5"},
                    correctAnswer = "x = 5",
                    questionDifficulty = "Hard"
                },
                new()
                {
                    question = "30 - 15 = ?",
                    answers = new string[] {"25", "20", "10", "15"},
                    correctAnswer = "15",
                    questionDifficulty = "Easy"
                },
                new()
                {
                    question = "60 \u00F7 3 = ?",
                    answers = new string[] {"14", "22", "20", "18"},
                    correctAnswer = "20",
                    questionDifficulty = "Easy"
                }
            };

            // Areas configuration
            var underWallArea = GetNode<Area2D>("Regions/UnderBarrierArea");
            var endpointArea = GetNode<Area2D>("Regions/EndpointArea");

            underWallArea.BodyEntered += (Node2D b) => OnUnderWallArea(b, true);
            underWallArea.BodyExited += (Node2D b) => OnUnderWallArea(b, false);
            endpointArea.BodyEntered += OnEndpointArea;

            // MathUI configuration
            Ui.AddQuestions(questions);
            Ui.SyncInventory(_inventory);
            Ui.OnAnswerCorrect = OnAnswerCorrect;
            Ui.CheckRecipe = RecipeChecker;
            Ui.SetPlayableArea(GetNode<Area2D>("Regions/PlayableArea"));
            Ui.DropConclusion = AfterDrop;
            Ui.SyncHealthProgress(WizardHealth);
        }

        private void OnAnswerCorrect(string difficulty)
        {
            var earnedItem = difficulty switch
            {
                "Easy" => EasyAnswerItem,
                "Medium" => MediumAnswerItem,
                "Hard" => HardAnswerItem,
                _ => null
            };
            _inventory.Add(earnedItem);
            answersAnsweredCorrectly++;

            if (answersAnsweredCorrectly == 1 && FirstTimePlaying)
            {
                Engine.TimeScale = 0.25f;
                Ui.OpenDialogue(FirstParchmentDialogue, () =>
                {
                    Engine.TimeScale = 1;
                    Ui.OpenParchment(earnedItem);
                    Input.MouseMode = Input.MouseModeEnum.Visible;
                });
            } else Ui.AddToParchment(earnedItem);
        }

        private void RecipeChecker(Items.Item[] itemsCombination)
        {
            foreach (var spell in PossibleSpells)
            {
                var itemsFound = 0;
                var tempArray = spell.CraftRecipe.ToList();

                for (int i = 0; i < 2; i++)
                if (tempArray.Contains(itemsCombination[i]))
                {
                    itemsFound++;
                    tempArray.Remove(itemsCombination[i]);
                }

                if (itemsFound == 2)
                {
                    _inventory.Remove(itemsCombination[0]);
                    _inventory.Remove(itemsCombination[1]);
                    _inventory.Add(spell);
                    return;
                }
            }
        }

        private void AfterDrop(Node2D spellNode, Items.MathSpell item)
        {
            var wizPlayer = WizardNode?.GetNode<AnimationPlayer>("AnimationPlayer");
            if (item.Name["en_US"] == "Storm")
                wizPlayer?.Play("spell_two");
            else wizPlayer?.Play("spell_one");
            GetNode("Battlefield").AddChild(spellNode);
            spellNode.Position = GetLocalMousePosition();
            if (!UnlimitedSpells) _inventory.Remove(item);
        }

        private void OnUnderWallArea(Node2D body, bool entered)
        {
            var wallAnPlayer = GetNode<AnimationPlayer>("Wall/AnimationPlayer");

            if (body is not MathEnemy enemy) return;
            if (entered)
            {
                _enemiesUnderWall.Add(enemy);
                if (_enemiesUnderWall.Count == 1)
                    wallAnPlayer.Play("transparency_on");
            }
            else
            {
                _enemiesUnderWall.Remove(enemy);
                if (_enemiesUnderWall.Count == 0)
                    wallAnPlayer.Play("transparency_off");
            }
        }

        private void OnEndpointArea(Node2D body)
        {
            if (body is not MathEnemy enemy) return;

            WizardHealth -= enemy.Damage;
            enemy.QueueFree();
        }

        private void OnSpawnTime()
        {
            var enemyIndex = (int) Math.Round((EnemiesScenes.Count - 1) * GD.Randf());
            var desiredX = GD.RandRange(SpawnerXStart, SpawnerXEnd);
            var instance = EnemiesScenes[enemyIndex].Instantiate<MathEnemy>();
            instance.GlobalPosition = new Vector2((float) desiredX, SpawnerYOrigin);
            instance.Destination = GoalDestiny;
            GetNode("Battlefield").AddChild(instance);
            GetNode("Battlefield").MoveChild(instance, 0);
        }
    }
}
