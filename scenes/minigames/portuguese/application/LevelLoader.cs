using System;
using System.Linq;
using Game.Minigames.Portuguese.Domain;
using Game.Minigames.Portuguese.Dto;
using Godot;
using Newtonsoft.Json;

namespace Game.Minigames.Portuguese.Application
{
    public class LevelLoader
    {
        public static Level LoadLevel(string filePath)
        {
            var fileContent = FileAccess.GetFileAsString(filePath);
            var levelDto = JsonConvert.DeserializeObject<LevelDto>(fileContent) ?? throw new Exception("Level file is empty or invalid.");
            var levelSentences = levelDto.PossibleSentences.Select(s => new Sentence
            {
                Text = s.Sentence,
                ChallengeWords = [.. s.ChallengeWords.Select(w => new Word(w.Word, w.WrongChoices))]
            }).ToArray();

            return new Level(levelDto.Name, levelSentences);
        }
    }
}