using System.Collections.Generic;
using CefetGame.Gameplay;
using Newtonsoft.Json.Linq;
using Godot;
using Game.Entities;
using CefetGame.Ui;
using Game;
using Newtonsoft.Json;

namespace CefetGame.Misc
{
    public partial class DialogueSystem : Node
    {
        // TODO: Add rewards support when possible.

        [Signal]
        public delegate void DialogueFinishedEventHandler();

        public static DialogueSystem Instance { get; private set; }

        private NewDialogueUi ui;
        private RichTextLabel titleNode;
        private RichTextLabel bodyNode;
        private Character speakingCharacter;

        private Dictionary<string, DialoguePart[]> dialogues;
        private DialoguePart currentDialoguePart;
        private string currentDialogueCourse;
        private int dialoguePartsIndex = 0;

        public void StartDialogue(NewDialogueUi ui, string dialogueJsonPath)
        {
            this.ui = ui;
            var file = FileAccess.GetFileAsString(dialogueJsonPath);
            var json = JObject.Parse(file);
            json.Remove("$schema");

            dialogues = json.ToObject<Dictionary<string, DialoguePart[]>>();

            currentDialogueCourse = "start"; // There must always be a "start" course!
            currentDialoguePart = dialogues[currentDialogueCourse][0];
            PrepareCharInfo(currentDialoguePart);

            ui.StartDialogue(currentDialoguePart);
            ui.Advanced += AdvanceDialogue;
            ui.OptionChosen += OnOptionChosen;
        }

        public void StartDialogue(NewDialogueUi ui, string dialogueJsonPath, Character speakingCharacter)
        {
            this.speakingCharacter = speakingCharacter;
            StartDialogue(ui, dialogueJsonPath);
        }

        private void AdvanceDialogue()
        {
            if (currentDialoguePart.GoTo is not null)
            {
                dialoguePartsIndex = 0;
                currentDialogueCourse = currentDialoguePart.GoTo;
                currentDialoguePart = dialogues[currentDialogueCourse][0];
                PrepareCharInfo(currentDialoguePart);
                ui.ChangeDialogue(currentDialoguePart);
                return;
            }

            if (currentDialoguePart.GoToScene is not null)
            {
                var scene = currentDialoguePart.GoToScene;
                FinishDialogue();
                GetNode<Global>("/root/Global").TransitionToScene(
                    scene,
                    style: TransitionStyleEnum.Circle
                    );

                return;
            }

            dialoguePartsIndex++;
            if (dialoguePartsIndex < dialogues[currentDialogueCourse].Length)
            {
                currentDialoguePart = dialogues[currentDialogueCourse][dialoguePartsIndex];
                PrepareCharInfo(currentDialoguePart);
                ui.ChangeDialogue(currentDialoguePart);
                return;
            }

            FinishDialogue();
        }

        private void FinishDialogue()
        {
            dialoguePartsIndex = 0;
            currentDialogueCourse = null;
            currentDialoguePart = null;
            speakingCharacter = null;

            ui.Advanced -= AdvanceDialogue;
            ui.Finish();
            EmitSignal(SignalName.DialogueFinished);
        }

        private void PrepareCharInfo(DialoguePart dialoguePart)
        {
            if (speakingCharacter is null)
                return;

            if (dialoguePart.Title == "{character}")
            {
                dialoguePart.Title = speakingCharacter.Name;
            }

            if (speakingCharacter.ImagesPath == "" || speakingCharacter.ImagesPath is null)
                return;

            var imgPath = GetCharacterImagePath(dialoguePart.Mood);
            if (ResourceLoader.Exists(imgPath))
            {
                var img = ResourceLoader.Load<Texture2D>(imgPath);
                ui.ChangeImage(img);
                return;
            }

            ui.ChangeImage(null);
        }

        private string GetCharacterImagePath(CharacterMood mood)
        {
            string imagePath = mood switch
            {
                CharacterMood.Happy => speakingCharacter.ImagesPath + "/happy.png",
                CharacterMood.Angry => speakingCharacter.ImagesPath + "/angry.png",
                CharacterMood.Nervous => speakingCharacter.ImagesPath + "/nervous.png",
                CharacterMood.Confused => speakingCharacter.ImagesPath + "/confused.png",
                CharacterMood.Reflexive => speakingCharacter.ImagesPath + "/reflexive.png",
                _ => speakingCharacter.ImagesPath + "/neutral.png"
            };

            return imagePath;
        }

        private void OnOptionChosen(string choiceJson)
        {
            var choice = JsonConvert.DeserializeObject<DialogueChoice>(choiceJson);
            if (choice.GoTo is not null)
            {
                dialoguePartsIndex = 0;
                currentDialogueCourse = choice.GoTo;
                currentDialoguePart = dialogues[currentDialogueCourse][0];
                PrepareCharInfo(currentDialoguePart);
                ui.ChangeDialogue(currentDialoguePart);
                return;
            }

            if (choice.GoToScene is not null)
            {
                FinishDialogue();
                GetNode<Global>("/root/Global").TransitionToScene(
                    choice.GoToScene,
                    style: TransitionStyleEnum.Circle
                    );

                return;
            }
        }

        public override void _Ready()
        {
            Instance = this;
        }
    }
}
