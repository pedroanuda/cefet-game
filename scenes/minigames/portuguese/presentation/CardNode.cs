using Game.Minigames.Portuguese.Domain;
using Godot;

namespace Game.Minigames.Portuguese.Presentation;

public partial class CardNode : Node2D
{
    [Export(PropertyHint.Dir)] public string CardsSpritesPath { get; set; } = "res://assets/misc/portuguese";

    public Card CardData { get; set; } = null!;

    private Sprite2D spriteNode = null!;
    private bool isSelected = false;

    public override void _Ready()
    {
        spriteNode = GetNode<Sprite2D>("Sprite");
        var spriteFile = CardData?.Type switch
        {
            CardType.Tree => "tree_card.png",
            CardType.Water => "water_card.png",
            CardType.Wind => "wind_card.png",
            CardType.Hint => "hint_card.png",
            _ => null
        };
        if (spriteFile != null)
            spriteNode.Texture = GD.Load<Texture2D>($"{CardsSpritesPath}/{spriteFile}");

        if (CardData != null)
        {
            var label = GetNode<Label>("Label");
            label.Text = CardData.WrittenText;
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        var tween = CreateTween();
        tween.TweenProperty(this, "position:y", selected ? -20f : 0f, 0.2f);
        tween.TweenProperty(this, "modulate", selected ? new Color("#e1f5f3") : Colors.White, 0.2f);
    }

    public async void TintError()
    {
        var originalModulate = Modulate;

        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color("#e7bcb6ff"), 0.25);

        await ToSignal(tween, Tween.SignalName.Finished);
        var tween2 = CreateTween();
        tween2.TweenProperty(this, "modulate", originalModulate, 0.25f);
    }

    public bool ContainsPoint(Vector2 point)
    {
        var textureOffset = spriteNode.Texture.GetSize() * spriteNode.Scale;
        var rect = new Rect2(spriteNode.GlobalPosition - textureOffset / 2, textureOffset);
        return rect.HasPoint(point);
    }
}
