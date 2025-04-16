using System.Collections.Generic;
using System;
using Godot;
using System.Linq;
using Game.Gameplay;
using Extensions;

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
        private int _defeatedEnemies = 0;
        private double _startTime = 0;

        // Base Items
        [Export] Camera2D Camera;
        [Export] public int WizardHealth 
        { 
            get => _health;
            set {
                if (value > 0)
                    _health = value;
                else
                {
                    _health = 0;
                    Ui.TriggerGameOver();
                }
                Ui?.UpdateHealthProgress(_health);
            }
        }
        [Export] public Items.Item EasyAnswerItem { get; set; }
        [Export] public Items.Item MediumAnswerItem { get; set; }
        [Export] public Items.Item HardAnswerItem { get; set; }
        [Export(PropertyHint.File, "*.json")]
        public string QuestionsFile { get; set; }
        private int _score = 0;
        private int _totalScore = 0;
        private int Score { get => _score; set {_score = value >= 0 ? value : 0;}}

        // Rounds Handling
        [Export] public Godot.Collections.Array<MathRound> Rounds { get; set; }
        private Timer _nextRoundTimer;
        private int _enemiesAlive = 0;
        private int _enemiesInRoundCount = 0;
        private int _currentRound = 0;

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
            _startTime = Time.GetUnixTimeFromSystem();

            var file_string = FileAccess.GetFileAsString(QuestionsFile);
            var parsedQuestions = Json.ParseString(file_string).AsGodotDictionary()["1_ano"];
            var questions = ConvertQuestions(parsedQuestions);

            // Areas configuration
            var underWallArea = GetNode<Area2D>("Regions/UnderBarrierArea");
            var endpointArea = GetNode<Area2D>("Regions/EndpointArea");

            underWallArea.BodyEntered += (Node2D b) => OnUnderWallArea(b, true);
            underWallArea.BodyExited += (Node2D b) => OnUnderWallArea(b, false);
            endpointArea.BodyEntered += OnEndpointArea;

            _nextRoundTimer = GetNode<Timer>("NextRoundTimer");
            _nextRoundTimer.Timeout += NextRound;

            // MathUI configuration
            Ui.AddQuestions(questions);
            Ui.SyncInventory(_inventory);
            Ui.OnAnswerCorrect = OnAnswerCorrect;
            Ui.CheckRecipe = RecipeChecker;
            Ui.SetPlayableArea(GetNode<Area2D>("Regions/PlayableArea"));
            Ui.DropConclusion = AfterDrop;
            Ui.SyncHealthProgress(WizardHealth);
            Ui.SetRoundLabelText($"Rodada 1/{Rounds.Count}");
        }

        private void OnAnswerCorrect(string difficulty)
        {
            var earnedItem = difficulty switch
            {
                "easy" => EasyAnswerItem,
                "medium" => MediumAnswerItem,
                "hard" => HardAnswerItem,
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

        private bool RecipeChecker(Items.Item[] itemsCombination)
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
                    return true;
                }
            }
            return false;
        }

        private void AfterDrop(Node spellNode, Items.MathSpell item)
        {
            var isGamepad = GetNode<Global>("/root/Global").GamepadOn;
            // Wizard Animation
            var wizPlayer = WizardNode?.GetNode<AnimationPlayer>("AnimationPlayer");
            if (item.Name["en_US"] == "Storm")
                wizPlayer?.Play("spell_two");
            else wizPlayer?.Play("spell_one");

            // Spell Handling
            GetNode("Battlefield").AddChild(spellNode);
            if (spellNode is MathFreezingBreath breath)
            {
                breath.Start = new Vector2(
                    isGamepad ? Ui.PreviewPosition.X : GetGlobalMousePosition().X,
                    520
                );
                breath.Limit = -20;
            }
            else if (spellNode is MathLaserBeam laser)
                laser.SpellStartingLocation = new Vector2(641, 448);
            else if (spellNode is MathFireball fireball)
            {
                fireball.Destiny = isGamepad ? Ui.PreviewPosition : GetGlobalMousePosition();
                fireball.SurgementPoint = new Vector2(
                    isGamepad ? Ui.PreviewPosition.X : GetGlobalMousePosition().X,
                    520
                );
            }
            else if (spellNode is Node2D)
                (spellNode as Node2D).Position = isGamepad ? Ui.PreviewPosition : GetLocalMousePosition();
            else if (spellNode is MathMeteor meteor)
            {
                meteor.MeteorDestiny = isGamepad ? Ui.PreviewPosition : GetGlobalMousePosition();
                meteor.Camera = Camera;
            }
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
            _enemiesAlive--;
            Score -= 5;
            if (_enemiesAlive <= 0)
                _nextRoundTimer.Start();
        }

        private void OnSpawnTime()
        {
            if (_enemiesInRoundCount >= Rounds[_currentRound].EnemiesAmount) return;

            var enemyIndex = (int) Math.Round((EnemiesScenes.Count - 1) * GD.Randf());
            var desiredX = GD.RandRange(SpawnerXStart, SpawnerXEnd);
            var instance = EnemiesScenes[enemyIndex].Instantiate<MathEnemy>();
            instance.GlobalPosition = new Vector2((float) desiredX, SpawnerYOrigin);
            instance.Destination = GoalDestiny;
            GetNode("Battlefield").AddChild(instance);
            GetNode("Battlefield").MoveChild(instance, 0);
            _enemiesInRoundCount++;
            _enemiesAlive++;

            instance.Died += () => CallDeferred(MethodName.CheckEnemies);
        }

        private void CheckEnemies()
        {
            _enemiesAlive--;
            _defeatedEnemies++;
            Score += 10;
            if (_enemiesInRoundCount == Rounds[_currentRound].EnemiesAmount && _enemiesAlive < 1)
                _nextRoundTimer.Start();
        }

        private void NextRound()
        {
            if (_currentRound + 1 >= Rounds.Count)
            {
                Win();
                return;
            }
            _currentRound++;
            _enemiesInRoundCount = 0;
            Ui.SetRoundLabelText($"Rodada {_currentRound + 1}/{Rounds.Count}");
            Ui.TriggerMainRoundLabel($"Rodada {_currentRound + 1}");
        }

        private void Win()
        {
            var timeDifference = (int) (Time.GetUnixTimeFromSystem() - _startTime);
            var timeScore = 1000 / (timeDifference % 60);
            _totalScore = Score + _defeatedEnemies;
            _totalScore += timeScore > 0 ? timeScore : 0;
            Ui.TriggerVictoryScreen(_score, _defeatedEnemies, timeDifference, _totalScore);
            
        }

        private List<QuestionAndAnswers> ConvertQuestions(Variant rawQuestions)
        {
            List<QuestionAndAnswers> convertedQuestions = new();
            var qs = rawQuestions.AsGodotDictionary<string, Godot.Collections.Array<Godot.Collections.Dictionary>>();
            var difficulties = new string[] { "easy", "medium", "hard" };

            foreach (var difficulty in difficulties)
                foreach (var q in qs[difficulty])
                    convertedQuestions.Add(new QuestionAndAnswers
                    {
                        question = q["question"].AsString(),
                        questionDifficulty = difficulty,
                        answers = q["answers"].AsStringArray(),
                        correctAnswer = q["correctAnswer"].AsString()
                    });
            return convertedQuestions.Shuffle() as List<QuestionAndAnswers>;
        }
    }
}
