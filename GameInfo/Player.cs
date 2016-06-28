using System;
using System.Collections.Generic;
using System.Linq;
using Hanabi.CardsInfo;

namespace Hanabi.GameInfo
{
    public class Player
    {
        public readonly List<HanabiCard> MyCards;

        public Player TeamMate { get; private set; }

        public Player()
        {
            MyCards = new List<HanabiCard>();
        }

        public void AddTeamMateInformation(Player teamMatePlayer)
        {
            TeamMate = teamMatePlayer;
        }

        public void PlayCard(int cardNumber, GameField gameField)
        {
            var card = ExtractCard(cardNumber);
            gameField.PutOnTheTable(card);
            UpdateKnowledge(cardNumber);
            AddCard(gameField.GetCardFromDeck());
        }

        public void DropCard(int cardNumber, GameField gameField)
        {
            ExtractCard(cardNumber);
            UpdateKnowledge(cardNumber);
            AddCard(gameField.GetCardFromDeck());
        }

        private void UpdateKnowledge(int cardNumber)
        {
            var cardsToUpdate = MyCards
                .Where(card => card.Knowledge.CardNumber > cardNumber)
                .Select(card => card);
            foreach (var hanabiCard in cardsToUpdate)
                hanabiCard.Knowledge.DecreaseCardNumber();
        }

        public void ListenTeamMate<T>(T cardInfo, int[] cardNumbers)
        {
            foreach (var cardNumber in cardNumbers)
            {
                LookAtCard(cardNumber).Knowledge.UpdateKnowledge(cardInfo);
                foreach (var hanabiCard in MyCards)
                {
                    var cardN = hanabiCard.Knowledge.CardNumber;
                    if (cardNumbers.Contains(cardN))
                        continue;
                    hanabiCard.Knowledge.UpdateNotKnowledge(cardInfo);
                }
            }
        }

        private HanabiCard ExtractCard(int cardNumber)
        {
            if (cardNumber > MyCards.Count || cardNumber < 0)
                throw new ArgumentException();
            var card = MyCards.ElementAt(cardNumber);
            MyCards.RemoveAt(cardNumber);
            return card;
        }

        public HanabiCard LookAtCard(int cardNumber)
        {
            if (cardNumber > MyCards.Count || cardNumber < 0)
                throw new ArgumentException();
            return MyCards.ElementAt(cardNumber);
        }

        public void AddCard(HanabiCard card)
        {
            MyCards.Add(card);
            card.Knowledge.AddNew(MyCards.IndexOf(card), card.Knowledge.RankKnowledge, card.Knowledge.ColorKnowledge);
        }
    }
}
