using Newtonsoft.Json;

namespace Game.Minigames.Portuguese.Dto
{
    public class PossibleSentenceDto
    {
        [JsonProperty("sentence", Required = Required.Always)]
        public string Sentence { get; set; }

        [JsonProperty("challengeWords", Required = Required.Always)]
        public WordDto[] ChallengeWords { get; set; }
    }

    public class WordDto
    {
        [JsonProperty("word", Required = Required.Always)]
        public string Word { get; set; }

        [JsonProperty("wrongChoices", Required = Required.Always)]
        public string[] WrongChoices { get; set; }
    }

    public class LevelDto
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("possibleSentences", Required = Required.Always)]
        public PossibleSentenceDto[] PossibleSentences { get; set; }
    }
}