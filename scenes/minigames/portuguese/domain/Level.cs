using System;

namespace Game.Minigames.Portuguese.Domain
{
    public class Level(string name, Sentence[] possibleSentences)
    {
        public string Name { get; set; } = name;
        public Sentence[] PossibleSentences { get; private set; } = possibleSentences;
        public bool IsCompleted { get; set; } = false;

        public Sentence GetRandomSentence()
        {
            if (PossibleSentences == null || PossibleSentences.Length == 0)
                return null;

            var random = new Random();
            var randomIndex = random.Next(PossibleSentences.Length);
            var sentence = PossibleSentences[randomIndex];

            return sentence;
        }
    }
}