using Newtonsoft.Json;

namespace CefetGame.Gameplay {
    public partial class DialogueChoice
    {
        [JsonProperty("text", Required = Required.Always)]
        public string Text { get; set; }

        [JsonProperty("goTo")]
        public string GoTo { get; set; }

        [JsonProperty("goToScene")]
        public string GoToScene {  get; set; }

        [JsonProperty("rewardId")]
        public int RewardId {  get; set; }
    }
}