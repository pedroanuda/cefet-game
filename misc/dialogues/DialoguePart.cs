using Game.Entities;
using Newtonsoft.Json;

namespace CefetGame.Gameplay
{
    public partial class DialoguePart
    {
        [JsonProperty("body", Required = Required.Always)]
        public string Body {  get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("choices")]
        public DialogueChoice[] Choices { get; set; }

        [JsonProperty("goTo")]
        public string GoTo { get; set; }

        [JsonProperty("goToScene")]
        public string GoToScene { get; set; }

        [JsonProperty("rewardId")]
        public int RewardId { get; set; }
        
        [JsonProperty("mood")]
        public CharacterMood Mood { get; set; }
    }
}
