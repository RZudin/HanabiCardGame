using System;
using System.Collections.Generic;
using System.Linq;
using Hanabi.GameInfo;

namespace Hanabi.CardsInfo
{
    public class CardKnowledges<TCardInfo, T>
            where TCardInfo : CardInfo<T>, new()
    {
        public TCardInfo KnownParameterIs { get; private set; }
        public List<TCardInfo> KnownParameterIsNot { get; private set; }

        public void UpdateKnowledge(TCardInfo cardInfo)
        {
            if (cardInfo != null)
                KnownParameterIs = cardInfo;
        }

        public bool TryUpdate<TInput>(TInput cardInfo)
        {
            if (typeof(CardInfo<T>) == typeof(TInput))
            {
                UpdateKnowledge((TCardInfo)(object)cardInfo);
                return true;
            }
            return false;
        }

        public bool TryUpdateNot<TInput>(TInput cardInfo, IEnumerable<T> collection)
        {
            if (typeof(CardInfo<T>) == typeof(TInput))
            {
                UpdateNotCardKnowledge((TCardInfo)(object)cardInfo, collection);
                return true;
            }
            return false;
        }

        public CardKnowledges()
        {
            KnownParameterIsNot = new List<TCardInfo>();
        }

        public void UpdateNotCardKnowledge(TCardInfo cardInfo, IEnumerable<T> allCardInfoValues)
        {
            if (cardInfo != null)
            {
                if (!KnownParameterIsNot.Contains(cardInfo))
                {
                    KnownParameterIsNot.Add(cardInfo);
                    var cardInfoValues = allCardInfoValues.ToList();

                    //if we have enough information about cards we will
                    //get right parameter using method of exclusion 
                    if (KnownParameterIsNot.Count == cardInfoValues.Count() - 1)
                    {
                        var infoToSave = cardInfoValues
                           .Where(cardInfoValue => !KnownParameterIsNot
                               .Select(localCardInfo => localCardInfo.GetCardInfo())
                               .Contains(cardInfoValue))
                           .Select(cardInfoValue => cardInfoValue)
                           .First();
                        KnownParameterIsNot.Clear();
                        var newInfo = new TCardInfo();
                        newInfo.SetInfoByString(infoToSave.ToString());
                        UpdateKnowledge(newInfo);
                    }
                }
            }
        }
    }

    public class CardKnowledge
    {
        public int CardNumber { get; private set; }
        public CardKnowledges<CardRank, int> RankKnowledge { get; private set; }
        public CardKnowledges<CardColor, CardColors> ColorKnowledge { get; private set; }

        public CardKnowledge()
        {
            CardNumber = 0;
            RankKnowledge = new CardKnowledges<CardRank, int>();
            ColorKnowledge = new CardKnowledges<CardColor, CardColors>();
        }

        public void UpdateKnowledge<T>(T cardInfo)
        {
            if (!RankKnowledge.TryUpdate(cardInfo))
                ColorKnowledge.TryUpdate(cardInfo);
        }

        public void UpdateNotKnowledge<T>(T cardInfo)
        {
            if (!RankKnowledge.TryUpdateNot(cardInfo, Enumerable.Range(1, HanabiCardLimits.MaxRank)))
                ColorKnowledge.TryUpdateNot(cardInfo, Enum.GetValues(typeof(CardColors))
                                                            .OfType<CardColors>());
        }

        public void DecreaseCardNumber()
        {
            CardNumber--;
        }

        public void AddNew(int cardNumber, CardKnowledges<CardRank, int> rankKnowledge, CardKnowledges<CardColor, CardColors> colorKnowledge)
        {
            CardNumber = cardNumber;
            RankKnowledge = rankKnowledge;
            ColorKnowledge = colorKnowledge;
        }

        public override string ToString()
        {
            return RankKnowledge + "" + ColorKnowledge;
        }
    }
}
