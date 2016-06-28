namespace Hanabi.CardsInfo
{
    public abstract class CardInfo<T>
    {
        public T CardParameter { get; protected set; }

        protected CardInfo(T parameter)
        {
            CardParameter = parameter;
        }

        protected CardInfo()
        {
        }

        public void SetInfoByString(string cardInfo)
        {
            CardParameter = GetCardInfoByString(cardInfo);
        }

        public T GetCardInfo()
        {
            return CardParameter;
        }

        public abstract T GetCardInfoByString(string cardInfo);
        public abstract bool EqualsToCard(HanabiCard card);
    }
}
