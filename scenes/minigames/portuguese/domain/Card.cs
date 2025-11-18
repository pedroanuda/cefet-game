namespace Game.Minigames.Portuguese.Domain
{
    public enum CardType
    {
        Tree,
        Water,
        Wind,
        Hint,
    }

    public class Card
    {
        public CardType Type { get; set; }
        public Word RelatedWord { get; set; }
        public string WrittenText { get; set; }
        public bool IsInSentence { get; set; }
    }
}