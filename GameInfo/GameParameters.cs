using System;
using System.Collections.Generic;

namespace Hanabi.GameInfo
{
    public static class Commands
    {
        public const string StartNewGame = @"Start new game with deck (.*)";
        public const string PlayCard = @"Play card (\d)";
        public const string DropCard = @"Drop card (\d)";
        public const string TellRank = @"Tell rank (\d) for cards (.*)";
        public const string TellColor = @"Tell color (\w+) for cards (.*)";
    }

    public enum CardColors
    {
        Red,
        Green,
        Blue,
        White,
        Yellow
    }

    public static class HanabiCardLimits
    {
        public static Dictionary<string, CardColors> CardAbbreviation { get; private set; }
        public const int MaxRank = 5;

        static HanabiCardLimits()
        {
            FillCardAbbreviations();
        }

        private static void FillCardAbbreviations()
        {
            CardAbbreviation = new Dictionary<string, CardColors>(StringComparer.CurrentCultureIgnoreCase)
            {
                {"R", CardColors.Red},
                {"G", CardColors.Green},
                {"B", CardColors.Blue},
                {"W", CardColors.White},
                {"Y", CardColors.Yellow}
            };
        }
    }
}
