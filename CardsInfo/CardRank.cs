using System;
using Hanabi.GameInfo;

namespace Hanabi.CardsInfo
{
    public class CardRank : CardInfo<int>
    {
        public CardRank()
        {
        }

        public CardRank(int rank)
            : base(rank)
        {
            if (rank < 0 || rank > HanabiCardLimits.MaxRank)
                throw new ArgumentException();
            CardParameter = rank;
        }

        public override int GetCardInfoByString(string cardInfo)
        {
            int resultRank;
            if (!int.TryParse(cardInfo, out resultRank))
                throw new ArgumentException();
            return resultRank;
        }

        public override bool EqualsToCard(HanabiCard card)
        {
            return CardParameter.Equals(card.GetRank());
        }

        public override string ToString()
        {
            return CardParameter.ToString();
        }
    }
}
