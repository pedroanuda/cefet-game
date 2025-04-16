using Godot;
using System;
using System.Collections.Generic;

namespace Game.UI
{
    public partial class MathParchment : Control
    {
        [Export] public Items.Item ItemToExhibitCraft { get; set; }
        [ExportGroup("Internal Logic")]
        [Export(PropertyHint.Dir)] public string SpellsDir { get; set; }
        [Export(PropertyHint.File, "*.tscn,*.scn")] public string GuideRowScenePath { get; set; }
        [Export] public Panel ModulatePanel { get; set; }

        public bool Opened { get => opened; }

        private Button openButton;
        private bool opened = false;
        private bool mouseInside = false;
        private List<Items.Item> knownParchments = new();

        public void ShowRecipesWith(Items.Item item)
        {
            knownParchments.Clear();
            AddRecipesWith(item);
            Toggle();
        }

        public void AddRecipesWith(Items.Item item)
        {
            if (knownParchments.Contains(item) || item == null) return;

            knownParchments.Add(item);
            UpdateContainer();
        }

        public void Toggle()
        {
            var anPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            if (anPlayer.IsPlaying()) return;

            AdjustAnimations(anPlayer);
            anPlayer.Play(opened ? "close" : "open");
            opened = !opened;

            if (!opened) Engine.TimeScale = 1;
            openButton.Text = opened ? "<" : ">";
        }

        private void UpdateContainer()
        {
            var SpellsAvailable = DirAccess.GetFilesAt(SpellsDir);
            var container = GetNode<VBoxContainer>("%RecipesContainer");
            foreach (var child in container.GetChildren())
            {
                container.RemoveChild(child);
                child.Free();
            }

            var knownSpells = new List<Items.MathSpell>();
            foreach (var spellFile in SpellsAvailable)
            {
                if (!(spellFile.EndsWith(".tres") || spellFile.EndsWith(".tres.remap"))) continue;
                var sf = spellFile.Replace(".remap", "");

                var spell = ResourceLoader.Load<Items.MathSpell>($"{SpellsDir}/{sf}");
                foreach (var item in knownParchments)
                    if (spell.CraftRecipe.Contains(item) && !knownSpells.Contains(spell))
                    {
                        knownSpells.Add(spell);

                        var scn = ResourceLoader.Load<PackedScene>(GuideRowScenePath).Instantiate<GuideRow>();
                        scn.RecipeResult = spell;
                        scn.Update();
                        container.AddChild(scn);
                    }
            }
        }

        private void ControlTimeScale(StringName animName)
        {
            if (animName == "open") Engine.TimeScale = 0.25f;
            else if (animName == "close") Engine.TimeScale = 1;
        }

        private void AdjustAnimations(AnimationPlayer anPlayer)
        {
            // Parchment Position
            var viewPortSize = GetViewportRect().Size;

            var openAnimation = anPlayer.GetAnimation("open");
            var openTrack = openAnimation.FindTrack("Parchment:position", Animation.TrackType.Value);
            openAnimation.TrackSetKeyValue(openTrack, 0, new Vector2(190, viewPortSize.Y / 2));
            openAnimation.TrackSetKeyValue(openTrack, 1, viewPortSize / 2);

            var closeAnimation = anPlayer.GetAnimation("close");
            var closeTrack = closeAnimation.FindTrack("Parchment:position", Animation.TrackType.Value);
            closeAnimation.TrackSetKeyValue(closeTrack, 0, viewPortSize / 2);
            closeAnimation.TrackSetKeyValue(closeTrack, 1, new Vector2(190, viewPortSize.Y / 2));

            // Panel Modulate
            if (ModulatePanel is null) return;
            if (openAnimation.FindTrack($"{ModulatePanel.GetPath()}:modulate", Animation.TrackType.Value) != -1)
                return;
            AdjustPanelAnimation(openAnimation, closeAnimation);
            
        }

        private void AdjustPanelAnimation(Animation openAnimation, Animation closeAnimation)
        {
            var openPanelIndex = openAnimation.AddTrack(Animation.TrackType.Value);
            openAnimation.TrackSetPath(openPanelIndex, $"{ModulatePanel.GetPath()}:modulate");
            openAnimation.TrackInsertKey(openPanelIndex, 0, ModulatePanel.Modulate);
            openAnimation.TrackInsertKey(openPanelIndex, 0.6f, new Color("#FFFFFF"));

            var visibilityOpenPanel = openAnimation.AddTrack(Animation.TrackType.Value);
            openAnimation.TrackSetPath(visibilityOpenPanel, $"{ModulatePanel.GetPath()}:visible");
            openAnimation.TrackInsertKey(visibilityOpenPanel, 0, true);

            var closePanelIndex = closeAnimation.AddTrack(Animation.TrackType.Value);
            closeAnimation.TrackSetPath(closePanelIndex, $"{ModulatePanel.GetPath()}:modulate");
            closeAnimation.TrackInsertKey(closePanelIndex, 0, new Color("#FFFFFF"));
            closeAnimation.TrackInsertKey(closePanelIndex, 0.6f, new Color("#FFFFFF00"));

            var visibilityClosePanel = closeAnimation.AddTrack(Animation.TrackType.Value);
            closeAnimation.TrackSetPath(visibilityClosePanel, $"{ModulatePanel.GetPath()}:visibile");
            closeAnimation.TrackInsertKey(visibilityClosePanel, 0.6f, false);
        }

        public override void _Ready()
        {
            openButton = GetNode<Button>("OpenButton");
            openButton.Pressed += Toggle;

            GetNode<AnimationPlayer>("AnimationPlayer").AnimationFinished += ControlTimeScale;
        }
    }
}
