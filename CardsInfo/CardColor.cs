using System;
using Hanabi.GameInfo;

namespace Hanabi.CardsInfo
{
    public class CardColor : CardInfo<CardColors>
    {
        public CardColor()
        {
        }

        public CardColor(CardColors color)
            : base(color)
        {
        }

        public override CardColors GetCardInfoByString(string cardInfo)
        {
            if (HanabiCardLimits.CardAbbreviation.ContainsKey(cardInfo))
                return HanabiCardLimits.CardAbbreviation[cardInfo];

            CardColors resultColor;
            if (!Enum.TryParse(cardInfo, true, out resultColor))
                throw new ArgumentException();
            return resultColor;
        }

        public override bool EqualsToCard(HanabiCard card)
        {
            return CardParameter.Equals(card.GetColor());
        }
        public override string ToString()
        {
            return CardParameter.ToString();
        }
    }
}
