using System.Collections.Generic;
using Godot;

namespace Game
{
    public partial class Stats : RefCounted
    {
        [Signal]
        public delegate void ChangedEventHandler();

        private struct PatentAndNecessaryXp
        {
            public string patent;
            public int necessaryXp;

            public PatentAndNecessaryXp(string patent, int necessaryXp)
            {
                this.patent = patent;
                this.necessaryXp = necessaryXp;
            }
        }

        static Dictionary<int, PatentAndNecessaryXp> levelAttributes = new (){
            { 1, new PatentAndNecessaryXp("1o ano", 1000) },
            { 2, new PatentAndNecessaryXp("1o ano", 1500) },
            { 3, new PatentAndNecessaryXp("1o ano", 2000) },
            { 4, new PatentAndNecessaryXp("1o ano", 2500) },
            { 5, new PatentAndNecessaryXp("2o ano", 3000) }
        }; 

        private int level = 1;
        private int xpProgress = 0;
        private int tillNextLevel = 1000;
        private string patent = "1o Ano";

        public int Level { get => level; }
        public int Xp { get => xpProgress; }
        public int CurrentMaxXp { get => tillNextLevel; }
        public string Patent { get => patent; }

        public void AddXP(int amount)
        {
            if (xpProgress + amount >= tillNextLevel)
            {
                level++;
                xpProgress = 0;
                tillNextLevel = levelAttributes[level].necessaryXp;
                patent = levelAttributes[level].patent;
                return;
            }
            xpProgress += amount;

            EmitSignal(SignalName.Changed);
        }
    }
}
