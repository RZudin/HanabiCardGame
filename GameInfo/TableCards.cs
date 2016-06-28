using System.Linq;
using Hanabi.CardsInfo;

namespace Hanabi.GameInfo
{
    public class TableCards : HanabiDeck
    {
        public void PutInTheDeck(HanabiCard card)
        {
            var cardOnTableIndex = Cards
                    .Where(tableCard => tableCard.GetColor().Equals(card.GetColor()))
                    .Where(tableCard => tableCard.GetRank().Equals(card.GetRank() - 1))
                    .Select(tableCard => Cards.IndexOf(tableCard))
                    .FirstOrDefault();
            Cards[cardOnTableIndex] = card;
        }
    }
}
