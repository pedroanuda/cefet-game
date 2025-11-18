using System.Linq;

namespace Game.Minigames.Portuguese.Domain
{
    public class Sentence
    {
        public string Text { get; set; }
        public Word[] ChallengeWords { get; set; }
        public Word NextWordToDiscover => ChallengeWords.FirstOrDefault(w => w.UndiscoveredLettersCount > 0) ?? null!;

        public string GetMaskedSentence()
        {
            return string.Join("   ", ChallengeWords.Select(w => w.GetMaskedSentence()));
        }
    }
}