using System.Runtime.Serialization;

namespace Game.Entities
{
    public enum CharacterMood
    {
        [EnumMember(Value = "neutral")]
        Neutral,
        [EnumMember(Value = "nervous")]
        Nervous,
        [EnumMember(Value = "happy")]
        Happy,
        [EnumMember(Value = "confused")]
        Confused,
        [EnumMember(Value = "reflexive")]
        Reflexive,
        [EnumMember(Value = "angry")]
        Angry
    }
}