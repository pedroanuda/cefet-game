using Game.Minigames.Portuguese.Domain;
using Godot;
using System;
using System.Collections.Generic;

namespace Game.Minigames.Portuguese.Presentation;

public partial class CardManager : Node
{
    private Node2D cardsNode;

    private List<CardNode> cards = [];
    private int selectedIndex = 0;

    public Func<CardNode, bool> CardChosenHandler { get; set; } = (card) => false;

    public override void _Ready()
    {
        LoadCards();
        GetViewport().SizeChanged += CenterNodes;

        UpdateSelection();
        CenterNodes();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!cardsNode.Visible || cards.Count == 0)
            return;

        if (@event.IsActionPressed("ui_right"))
        {
            selectedIndex = (selectedIndex + 1) % cards.Count;
            UpdateSelection();
        }
        else if (@event.IsActionPressed("ui_left"))
        {
            selectedIndex = (selectedIndex - 1 + cards.Count) % cards.Count;
            UpdateSelection();
        }
        else if (@event.IsActionPressed("ui_accept"))
        {
            if (selectedIndex >= 0)
                CardChosenHandler(cards[selectedIndex]);
        }
        else if (@event is InputEventMouseMotion)
        {
            var pos = GetViewport().GetMousePosition();
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                if (cards[i].ContainsPoint(pos))
                {
                    selectedIndex = i;
                    UpdateSelection();
                    break;
                }
            }
        }
        else if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            var pos = GetViewport().GetMousePosition();
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                if (cards[i].ContainsPoint(pos))
                {
                    selectedIndex = i;
                    UpdateSelection();
                    CardChosenHandler(cards[i]);
                    break;
                }
            }
        }
    }

    public void LoadCards()
    {
        cardsNode = GetParent().GetNode<Node2D>("Cards");
        foreach (Node child in cardsNode.GetChildren())
        {
            if (child is CardNode card)
                cards.Add(card);
        }
        CenterNodes();
    }

    public void ClearCards()
    {
        foreach (var card in cards)
        {
            cardsNode.RemoveChild(card);
            card.QueueFree();
        }
        cards.Clear();
        selectedIndex = -1;
    }

    public void RemoveCard(CardNode card)
    {
        cardsNode.RemoveChild(card);
        cards.Remove(card);
        selectedIndex = Math.Clamp(selectedIndex, 0, cards.Count - 1);
        card.QueueFree();
        UpdateSelection();
        CenterNodes();
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < cards.Count; i++)
            cards[i].SetSelected(i == selectedIndex);
    }

    private void CenterNodes()
    {
        if (cardsNode == null || !cardsNode.Visible)
            return;

        var viewport = GetViewport();
        var visibleSize = viewport.GetVisibleRect().Size;
        cardsNode.Position = new Vector2(visibleSize.X / 2f, visibleSize.Y - 150f);

        if (cards.Count == 0)
            return;

        const float spacing = 25f; // espaçamento entre cartas
        float totalWidth = 0f;
        var widths = new List<float>(cards.Count);

        // Calcula larguras (considerando escala) e soma o total
        foreach (var card in cards)
        {
            var sprite = card.GetNodeOrNull<Sprite2D>("Sprite");
            Vector2 texSize = Vector2.Zero;
            Vector2 scale = Vector2.One;

            if (sprite != null && sprite.Texture != null)
            {
                texSize = sprite.Texture.GetSize() / 2;
                scale = sprite.Scale;
            }

            float width = texSize.X * scale.X;
            widths.Add(width);
            totalWidth += width;
        }

        // adiciona espaçamentos entre as cartas (N-1 espaçamentos)
        totalWidth += spacing * (cards.Count - 1);

        // Posiciona começando da esquerda para centralizar o conjunto
        float currentX = -totalWidth / 2f;
        for (int i = 0; i < cards.Count; i++)
        {
            float w = widths[i];
            float centerX = currentX + w / 2f;

            var tween = CreateTween();
            tween.TweenProperty(cards[i], "position:x", centerX, 0.25f);
            //cards[i].Position = new Vector2(centerX, 0f);
            currentX += w + spacing;
        }
    }
}
