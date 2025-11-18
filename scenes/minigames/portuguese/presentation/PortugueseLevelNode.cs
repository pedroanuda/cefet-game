using System.Threading.Tasks;
using CefetGame.Misc;
using CefetGame.Ui;
using Game.Minigames.Portuguese.Application;
using Game.Minigames.Portuguese.Domain;
using Godot;

namespace Game.Minigames.Portuguese.Presentation;

[GlobalClass]
public partial class PortugueseLevelNode : Node2D
{
    [Signal] public delegate void CutsceneStartedEventHandler(string levelName);
    [Signal] public delegate void LevelEndedEventHandler();
    [Signal] public delegate void LevelStartedEventHandler();

    [Export(PropertyHint.File, "*.json")] public string LevelInfo { get; set; } = "";
    [Export] public bool PlayCutsceneOnPlayerEnter { get; set; } = true;

    [ExportGroup("Dialogue")]
    [Export] public NewDialogueUi DialogueUi { get; set; }
    [Export(PropertyHint.File, "*.json")] public string DialoguePath { get; set; } = "";
    [Export(PropertyHint.File, "*.json")] public string CompletedDialoguePath { get; set; } = "";

    private PortuguesePlayer linkedPlayer;
    private Area2D playerDetectionArea;
    private Camera2D cutsceneCamera;
    private Path2D cameraPath;
    private AnimationPlayer animationPlayer;

    private bool isPathFollowing = false;
    private Level level;

    public Level Level => level;

    public override void _Ready()
    {
        level = LevelLoader.LoadLevel(LevelInfo);

        playerDetectionArea = GetNode<Area2D>("PlayerDetectionArea");
        playerDetectionArea.BodyEntered += OnPlayerDetected;
        cutsceneCamera = GetNode<Camera2D>("%CutsceneCamera");
        cameraPath = GetNode<Path2D>("CameraPath");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (isPathFollowing)
        {
            var pathFollow = cameraPath.GetNode<PathFollow2D>("PathFollow");
            pathFollow.ProgressRatio += (float)(delta * (1.0 / 1.5));

            if (pathFollow.ProgressRatio >= 1.0f)
                isPathFollowing = false;
        }
    }

    public void FinishLevel()
    {
        void animationCallback(StringName animName)
        {
            animationCallback2();
        }
        async Task animationCallback2()
        {
            EmitSignal(SignalName.LevelEnded);
            animationPlayer.AnimationFinished -= animationCallback;
            await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);
            StopCutscene();
        }
        if (animationPlayer.HasAnimation("solve"))
        {
            animationPlayer.AnimationFinished += animationCallback;
            animationPlayer.Seek(0, true);
            animationPlayer.Play("solve");
        }
        if (Name == "GrayRainbow")
        {
            var rainbow = GetParent().GetParent().GetNode<AnimatedSprite2D>("%Rainbow");
            rainbow.AnimationFinished += () => animationCallback(null);
            rainbow.Play("default");
        }
    }

    private void OnPlayerDetected(Node2D body)
    {
        if (body is not PortuguesePlayer player || level.IsCompleted || !PlayCutsceneOnPlayerEnter)
            return;

        this.linkedPlayer = player;
        StartCutscene();
    }

    private void StartCutscene()
    {
        var playerCamera = linkedPlayer.GetNodeOrNull<Camera2D>("Camera2D");
        if (playerCamera != null)
            playerCamera.Enabled = false;
        linkedPlayer.CanMove = false;

        if (playerCamera != null)
        {
            var playerCameraPosition = ToLocal(playerCamera.GlobalPosition);
            cameraPath.Curve.AddPoint(playerCameraPosition);
            cameraPath.Curve.AddPoint(cutsceneCamera.Position);
            isPathFollowing = true;
        }
        cutsceneCamera.Enabled = true;
        animationPlayer.Play("camera_zoom");
        EmitSignal(SignalName.CutsceneStarted, level.Name);

        SceneTreeTimer timer = GetTree().CreateTimer(1.5f);
        timer.Timeout += () =>
        {
            var dialogueToPlay = level.IsCompleted ? CompletedDialoguePath : DialoguePath;
            DialogueSystem.Instance.StartDialogue(DialogueUi, dialogueToPlay);
            void dialogueCallback()
            {
                DialogueSystem.Instance.DialogueFinished -= dialogueCallback;
                EmitSignal(SignalName.LevelStarted);
            }
            DialogueSystem.Instance.DialogueFinished += dialogueCallback;
        };
    }

    private void StopCutscene()
    {
        if (linkedPlayer == null)
        {
            GD.Print("Is null");
            return;
        }

        var playerCamera = linkedPlayer.GetNodeOrNull<Camera2D>("Camera2D");
        if (playerCamera != null)
            playerCamera.Enabled = true;
        cutsceneCamera.Enabled = false;
        linkedPlayer.CanMove = true;

        isPathFollowing = false;
        cameraPath.Curve.ClearPoints();
    }
}
