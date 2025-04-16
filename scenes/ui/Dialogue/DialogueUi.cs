using Game.Gameplay;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

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
            var container = GetNode<VBoxContainer>("%OptionsContainer");

            foreach (var item in container.GetChildren())
            {
                item.QueueFree();
            }

            container.Hide();
        }

        public override void _Ready()
        {
            textLabel = GetNode<RichTextLabel>("%TextBody");
            Visible = false;
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (!Visible || isAChooseDialogue) return;

            //var buttonsContainer = GetNode("%OptionsContainer").GetChildren();
            //if (@event.IsActionReleased("confirm") && isAChooseDialogue)
            //{
            //    //var chDialogue = (ChooseDialogue)(dialogues[dialogueIndex - 1]);
            //    if (buttonsContainer[0] is Button btn)
            //        btn.EmitSignal(Button.SignalName.Pressed);
            //    return;
            //}
            //else if (@event.IsActionReleased("backy_back") && isAChooseDialogue)
            //{
            //    if (buttonsContainer[1] is Button btn)
            //        btn.EmitSignal(Button.SignalName.Pressed);
            //}

            if ((@event is InputEventKey e && e.IsReleased()) || (@event is InputEventMouseButton e1 && e1.IsReleased())
                || @event.IsActionReleased("confirm"))
            {
                if (leftDialogues > 0)
                    Open(dialogues[dialogueIndex]);
                else
                {
                    if (finishAction is not null) finishAction();
                    Close();
                }
            }
            GetViewport().SetInputAsHandled();
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
                "{character}" => "Vit\u00f3ria",
                _ => dialogue.Title,
            };

            textLabel.Text = $"[b]{title}[/b]\n\n{dialogue.Body}";

            if (dialogue is ChooseDialogue dialogueChoosing)
            {
                isAChooseDialogue = true;
                Input.MouseMode = Input.MouseModeEnum.Visible;

                var container = GetNode<VBoxContainer>("%OptionsContainer");
                foreach (var item in dialogueChoosing.Options)
                {
                    var option = ResourceLoader.Load<PackedScene>("res://scenes/ui/Menu/menu_button.tscn").Instantiate<Button>();

                    option.Text = item.OptionText;
                    option.Pressed += () => ChooseOption(item);

                    container.AddChild(option);
                    option.SizeFlagsVertical = SizeFlags.ExpandFill;
                }
                container.Show();
                (container.GetChildren()[0] as Button).GrabFocus();
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
