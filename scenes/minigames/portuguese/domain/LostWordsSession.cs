using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Game.UI;
using Godot;

namespace Game.Minigames.Portuguese.Domain
{
	public struct GameStatistics(int errors, int hints, int total)
	{
		public int ErrorsMade { get; set; } = errors;
		public int HintsUsed { get; set; } = hints;
		public int TotalScore { get; set; } = total;
	}

	public class ComboStatus
	{
		public int CurrentCombo { get; set; } = 0;
		public CardType CurrentComboCardType;

		public bool RegisterRightCard(CardType cardType)
		{
			if (CurrentCombo == 0)
			{
				CurrentCombo++;
				CurrentComboCardType = cardType;
				return false;
			}

			if (cardType != CurrentComboCardType)
			{
				CurrentCombo = 1;
				CurrentComboCardType = cardType;
				return false;
			}

			CurrentCombo++;
			if (CurrentCombo >= 2)
			{
				CurrentCombo = 0;
				return true;
			}

			return false;
		}

		public void RegisterFailure()
		{
			CurrentCombo = 0;
		}
	}

	public class LostWordsSession
	{
		public Level CurrentLevel { get; set; }
		public Sentence CurrentSentence { get; set; }
		public List<Card> SpecialHandCards { get; set; } = [];
		public List<Card> HandCards { get; set; } = [];
		public List<Card> UsedCards { get; set; } = [];

		public int ClearedLevels { get; private set; }
		public int ErrorsMade { get; private set; } = 0;
		public int HintsUsed { get; private set; } = 0;

		private ComboStatus comboStatus = new();

		public void StartLevel(Level level)
		{
			CurrentLevel = level;
			CurrentSentence = level.GetRandomSentence();
			GoToNextWord();
		}

		public void GoToNextWord()
		{
			if (CurrentSentence == null)
				return;

			HandCards.Clear();
			GenerateCards();
		}

		public void FinishCurrentLevel()
		{
			ClearedLevels += 1;
			CurrentLevel.IsCompleted = true;
			CurrentLevel = null;
			HandCards.Clear();
			UsedCards.Clear();
		}

		public GameStatistics FinishGame()
		{
			int totalScore = (ClearedLevels * 200) - (HintsUsed * 15) - (ErrorsMade * 20);
			totalScore = totalScore >= 0 ? totalScore : 0;
			LeaderboardPanel.AddScore(totalScore, "Português");

			return new GameStatistics(ErrorsMade, HintsUsed, totalScore);
		}

		public bool TryCard(Card card)
		{
			if (CurrentSentence == null)
				return false;

			if (card.Type == CardType.Hint)
			{
				HintsUsed++;
				CurrentSentence.NextWordToDiscover.DiscoverRandomLetters(new Random().Next(1, 3));
				SpecialHandCards.Remove(card);
				return true;
			}

			bool isCorrect = card.RelatedWord.Text == card.WrittenText;
			if (isCorrect)
			{
				card.RelatedWord.DiscoverAllLetters();
				UsedCards.Add(card);
				HandCards.Remove(card);

				var isComboDone = comboStatus.RegisterRightCard(card.Type);
				if (isComboDone)
					ApplyComboResult(card.Type);
				return true;
			}

			ErrorsMade += 1;
			comboStatus.RegisterFailure();
			return false;
		}

		private void ApplyComboResult(CardType cardType)
		{
			if (cardType == CardType.Tree)
			{
				var wrongCards = HandCards.Where(c => c.WrittenText != c.RelatedWord.Text).ToList();
				var numToRemove = new Random().Next(1, Math.Min(3, wrongCards.Count));

				var cardsToRemove = wrongCards.OrderBy(x => Guid.NewGuid()).Take(numToRemove).ToList();
				foreach (var cardToRemove in cardsToRemove)
				{
					HandCards.Remove(cardToRemove);
				}
				return;
			}

			if (cardType == CardType.Water)
			{
				var hintCard = new Card
				{
					Type = CardType.Hint,
					RelatedWord = CurrentSentence.NextWordToDiscover,
					IsInSentence = false
				};

				SpecialHandCards.Add(hintCard);
				return;
			}

			if (cardType == CardType.Wind)
			{
				var undiscoveredWords = CurrentSentence.ChallengeWords.Where(w => w.UndiscoveredLettersCount > 0).ToList();
				if (undiscoveredWords.Count != 0)
				{
					var randomWord = undiscoveredWords[new Random().Next(undiscoveredWords.Count)];
					randomWord.DiscoverRandomLetters(new Random().Next(1, 3));
				}
			}
		}

		private void GenerateCards()
		{
			if (CurrentSentence == null || CurrentSentence.NextWordToDiscover == null)
				return;
			var nextWord = CurrentSentence.NextWordToDiscover;
			var cardsList = new List<Card>();


			var rightCard = new Card
			{
				Type = ChooseCardType(nextWord.Text),
				WrittenText = nextWord.Text,
				RelatedWord = nextWord,
				IsInSentence = false
			};
			cardsList.Add(rightCard);

			foreach (var wordChoiceText in nextWord.WrongChoices)
			{
				var card = new Card
				{
					Type = ChooseCardType(wordChoiceText),
					RelatedWord = nextWord,
					WrittenText = wordChoiceText,
					IsInSentence = true
				};
				cardsList.Add(card);
			}

			cardsList.Shuffle();
			if (HintsUsed == 0 && SpecialHandCards.Count == 0)
			{
				var hintCard = new Card
				{
					Type = CardType.Hint,
					RelatedWord = nextWord,
					IsInSentence = false
				};
				SpecialHandCards.Add(hintCard);
			}
			HandCards = [.. cardsList];
		}

		private static CardType ChooseCardType(string word)
		{
			var all = (CardType[])System.Enum.GetValues(typeof(CardType));
			var types = new List<CardType>();
			foreach (var t in all)
				if (t != CardType.Hint)
					types.Add(t);

			CardType chosenType = CardType.Hint;
			if (types.Count > 0)
			{
				var bytes = System.Text.Encoding.UTF8.GetBytes(word ?? "");
				uint hash = 2166136261u;
				for (int i = 0; i < bytes.Length; i++)
				{
					hash ^= bytes[i];
					hash *= 16777619u;
				}
				int idx = (int)(hash % (uint)types.Count);
				chosenType = types[idx];
			}
			return chosenType;
		}
	}
}
