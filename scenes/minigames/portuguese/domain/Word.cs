using System;
using System.Linq;

namespace Game.Minigames.Portuguese.Domain
{
    public class Word
    {
        private string text = string.Empty;

        public string Text
        {
            get => text;
            set
            {
                text = value;
                InitializeDiscoveredLetters();
            }
        }
        public char[] Letters => Text.ToCharArray();
        public char[] DiscoveredLetters { get; private set; } = [];
        public int UndiscoveredLettersCount => DiscoveredLetters.Count(c => c == '_');

        public string[] WrongChoices { get; private set; } = [];

        public Word(string text, string[] wrongChoices)
        {
            Text = text;
            WrongChoices = wrongChoices;
        }

        public string GetMaskedSentence()
        {
            return string.Join(" ", DiscoveredLetters);
        }

        public void DiscoverAllLetters()
        {
            DiscoveredLetters = Letters.ToArray();
        }

        public void DiscoverRandomLetters(int count)
        {
            var random = new Random();
            var undiscoveredIndices = Enumerable.Range(0, Letters.Length)
                                               .Where(i => DiscoveredLetters[i] == '_')
                                               .ToList();

            for (int i = 0; i < count && undiscoveredIndices.Count > 0; i++)
            {
                int randomIndex = random.Next(undiscoveredIndices.Count);
                int letterIndex = undiscoveredIndices[randomIndex];
                DiscoveredLetters[letterIndex] = Letters[letterIndex];
                undiscoveredIndices.RemoveAt(randomIndex);
            }
        }

        private void InitializeDiscoveredLetters()
        {
            DiscoveredLetters = [.. Enumerable.Repeat('_', text.Length)];
        }
    }
}