using System.Linq;
using Hanabi.GameInfo;

namespace Hanabi.CardsInfo
{
    public class HanabiCard
    {
        public readonly CardColor Color;
        public readonly CardRank Rank;
        public readonly CardKnowledge Knowledge;

        public HanabiCard(CardColor color, CardRank rank)
        {
            Color = color;
            Rank = rank;
            Knowledge = new CardKnowledge();
        }

        public override bool Equals(object obj)
        {
            var anotherCard = obj as HanabiCard;
            return (anotherCard != null && Color.Equals(anotherCard.Color)
                && Rank.Equals(anotherCard.Rank));
        }

        public override int GetHashCode()
        {
            return Color.GetHashCode() + Rank.GetHashCode();
        }

        public override string ToString()
        {
            return Color.CardParameter.ToString().ElementAt(0) + "" + Rank.CardParameter;
        }

        public CardColors GetColor()
        {
            return Color.GetCardInfo();
        }

        public int GetRank()
        {
            return Rank.GetCardInfo();
        }
    }

}
