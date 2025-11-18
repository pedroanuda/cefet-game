using System;
using System.Collections.Generic;
using System.Linq;
using Game.Minigames.Portuguese.Domain;
using Game.Minigames.Portuguese.Presentation;
using Godot;

namespace Game.Minigames.Portuguese;

public partial class LostWords : Node2D
{
    private Node2D cardsNode;
    private CardManager cardManager;
    private LostWordsUi ui;

    private Action onLevelEnded;

    private LostWordsSession Session { get; set; } = new();

    public override void _Ready()
    {
        cardsNode = GetNode<Node2D>("CardsLayer/Cards");
        cardManager = GetNode<CardManager>("CardsLayer/CardManager");
        ui = GetNode<LostWordsUi>("%LostWordsUi");
        GetNode("Levels").GetChildren().OfType<PortugueseLevelNode>().ToList().ForEach(levelNode =>
        {
            levelNode.CutsceneStarted += (levelName) => OnEnterLevel(levelNode.Level); ;
            levelNode.LevelStarted += () => OnStartLevel(levelNode);
        });

        cardManager.CardChosenHandler = OnCardChosen;
    }

    private bool OnCardChosen(CardNode card)
    {
        bool isCorrectCard = Session.TryCard(card.CardData);
        if (isCorrectCard)
        {
            ui.UpdateSentence(Session.CurrentSentence);
            cardManager.RemoveCard(card);

            if (Session.CurrentSentence.NextWordToDiscover == null)
            {
                cardsNode.Visible = false;

                Session.FinishCurrentLevel();
                onLevelEnded?.Invoke();
                return true;
            }

            Session.GoToNextWord();
            LoadNewCards();
            return true;
        }

        card.TintError();
        return false;
    }

    private void OnEnterLevel(Level levelInfo)
    {
        ui.EnterLevel(levelInfo);
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not PortuguesePlayer player)
            return;

        player.CanMove = false;
        var statistics = Session.FinishGame();
        ui.ShowEndScreen(statistics);
    }

    private void OnStartLevel(PortugueseLevelNode node)
    {
        var levelInfo = node.Level;
        Session.StartLevel(levelInfo);
        ui.ShowSentence(Session.CurrentSentence);

        LoadNewCards();
        onLevelEnded = () =>
        {
            ui.ExitLevel();
            node.FinishLevel();
        };
    }

    private void LoadNewCards()
    {
        cardManager.ClearCards();

        List<Card> cards = [.. Session.HandCards, .. Session.SpecialHandCards];
        foreach (var card in cards)
        {
            var cardNode = GD.Load<PackedScene>("res://scenes/minigames/portuguese/presentation/CardNode.tscn").Instantiate<CardNode>();
            cardNode.CardData = card;
            cardsNode.AddChild(cardNode);
        }

        cardsNode.Visible = true;
        cardManager.LoadCards();
    }
}