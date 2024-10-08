using Game.Gameplay;
using Godot;
using Godot.Collections;
using System;

namespace Game.UI
{
    public partial class DialogueUi : Control
    {
        private Action finishAction;
        private RichTextLabel textLabel;
        private Array<Dialogue> dialogues;
        private int dialogueIndex = 0;
        private int leftDialogues = 0;
        private bool isAChooseDialogue = false;
        private Vector2 TransitionOrigin;

        private void RemoveOptions()
        {
            var container = GetNode<HBoxContainer>("%OptionsContainer");

            foreach (var item in container.GetChildren())
            {
                item.QueueFree();
            }
        }

        public override void _Ready()
        {
            textLabel = GetNode<RichTextLabel>("%TextBody");
            Visible = false;
        }

        public override void _UnhandledKeyInput(InputEvent @event)
        {
            if (!Visible || isAChooseDialogue) return;

            if (@event is InputEventKey e && e.IsReleased() && e.Keycode == Key.Space)
            {
                if (leftDialogues > 0)
                    Open(dialogues[dialogueIndex]);
                else
                {
                    if (finishAction is not null) finishAction();
                    Close();
                }
            }
        }

        public void Open(DialogueCollection dialogueCollection, Action onFinish = null, Node2D origin = null)
        {
            if (origin is not null) TransitionOrigin = origin.GetTransform().Origin;

            void specialAction()
            {
                switch (dialogueCollection.EndingType)
                {
                    case (DialogueFinishing.SceneSwitch):
                        Input.MouseMode = Input.MouseModeEnum.Visible;

                        var handler = GetNode<TransitionHandler>("/root/TransitionHandler");
                        handler.StartingPosition = TransitionOrigin;
                        //
                        //void onHalf()
                        //{
                        //    GetNode<Global>("/root/Global")
                        //    .GoToScene(dialogueCollection.EndingExpression, () =>
                        //    handler.Play("change_to_minigame_end"));
                        //
                        //    handler.TransitionInHalf -= onHalf;
                        //}
                        //
                        //handler.TransitionInHalf += onHalf;
                        //handler.Play("change_to_minigame_start");
                        GetNode<Global>("/root/Global")
                        .TransitionToScene(
                            dialogueCollection.EndingExpression,
                            style: TransitionStyleEnum.Circle
                        );
                        break;
                    default:
                        break;
                }
            }

            if (finishAction is not null)
            {
                var aux = finishAction;
                finishAction = () =>
                {
                    aux?.Invoke();
                    specialAction();
                };

                dialogues.AddRange(dialogueCollection.Dialogues);
                leftDialogues += dialogueCollection.Dialogues.Count;
                Open(dialogues[dialogueIndex]);
            }
            else
            {
                finishAction = () => { onFinish?.Invoke(); specialAction(); };

                dialogues = dialogueCollection.Dialogues.Duplicate();
                leftDialogues = dialogues.Count;

                Open(dialogues[dialogueIndex], onFinish);
            }
        }

        public void Open(Dialogue dialogue, Action onFinish = null)
        {
            finishAction ??= onFinish;

            leftDialogues--;
            dialogueIndex++;

            string title = dialogue.Title switch
            {
                "{character}" => "Gustavo Black",
                _ => dialogue.Title,
            };

            textLabel.Text = $"[b]{title}[/b]\n\n{dialogue.Body}";

            if (dialogue is ChooseDialogue dialogueChoosing)
            {
                isAChooseDialogue = true;
                Input.MouseMode = Input.MouseModeEnum.Visible;

                foreach (var item in dialogueChoosing.Options)
                {
                    var option = new Button();
                    option.AddThemeStyleboxOverride(
                        "normal",
                        GD.Load<StyleBoxFlat>("res://scenes/ui/Dialogue/OptionStyleBox.tres")
                    );

                    option.Text = item.OptionText;
                    option.Pressed += () => ChooseOption(item);

                    GetNode<HBoxContainer>("%OptionsContainer").AddChild(option);
                }
            }
            else
            {
                isAChooseDialogue = false;
                Input.MouseMode = Input.MouseModeEnum.Hidden;
            }
            Show();
        }

        public void ChooseOption(DialogueOption option)
        {
            RemoveOptions();
            Open(option.CourseOfAction);
        }

        public void Close()
        {
            dialogueIndex = 0;
            finishAction = null;
            TransitionOrigin = Vector2.Zero;
            Hide();
        }
    }
}
