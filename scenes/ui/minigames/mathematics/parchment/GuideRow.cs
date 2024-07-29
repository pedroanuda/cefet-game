using Game.Items;
using Godot;
using System;

namespace Game.UI
{
    public partial class GuideRow : Control
    {
        [Export]
        public MathSpell RecipeResult;

        public void Update()
        {
            var Recipe = RecipeResult.CraftRecipe;

            GetNode<TextureRect>("%Item1").Texture = Recipe[0].Icon;
            GetNode<TextureRect>("%Item2").Texture = Recipe[1].Icon;
            GetNode<TextureRect>("%Result").Texture = RecipeResult.Icon;
        }

        public override void _Ready()
        {
            if (RecipeResult is not null) Update();
        }
    }
}
