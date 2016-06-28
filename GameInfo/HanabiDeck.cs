using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Hanabi.CardsInfo;

namespace Hanabi.GameInfo
{
    public class HanabiDeck
    {
        public readonly List<HanabiCard> Cards;

        public HanabiDeck(string cardDeck)
        {
            Cards = new List<HanabiCard>();
            var allCards = Regex.Matches(cardDeck, @"(\D+)(\d+)\s?");
            foreach (Match card in allCards)
            {
                var cardColor = new CardColor();
                cardColor.SetInfoByString(card.Groups[1].Value);
                var cardRank = new CardRank();
                cardRank.SetInfoByString(card.Groups[2].Value);
                Cards.Add(new HanabiCard(cardColor, cardRank));
            }
            Cards.Reverse();
        }

        public HanabiDeck()
        {
            Cards = new List<HanabiCard>();
            foreach (var card in HanabiCardLimits.CardAbbreviation)
            {
                var color = new CardColor(card.Value);
                var rank = new CardRank(0);
                Cards.Add(new HanabiCard(color, rank));
            }
        }

        public HanabiCard ExtractTop()
        {
            var topCard = Cards.LastOrDefault();
            if (topCard != null)
                Cards.RemoveAt(Cards.Count - 1);
            return topCard;
        }

        public int GetCardsCount()
        {
            return Cards.Count;
        }

        public override string ToString()
        {
            var resultString = new StringBuilder();
            foreach (var card in Cards)
            {
                var cardColor = HanabiCardLimits.CardAbbreviation
                    .Where(nameAndColor => nameAndColor.Value.Equals(card.GetColor()))
                    .Select(nameAndColor => nameAndColor.Key).FirstOrDefault();
                resultString.Append(cardColor + "" + card.Rank + " ");
            }
            return resultString.ToString();
        }
    }

}
