using Hanabi.CardsInfo;

namespace Hanabi.GameInfo
{
    public class GameField
    {
        public TableCards TableCards { get; set; }
        public readonly HanabiDeck DeckCards;

        public GameField(string cardDeck)
        {
            TableCards = new TableCards();
            DeckCards = new HanabiDeck(cardDeck);
        }

        public void PutOnTheTable(HanabiCard card)
        {
            TableCards.PutInTheDeck(card);
        }

        public HanabiCard GetCardFromDeck()
        {
            return DeckCards.ExtractTop();
        }
    }
}
