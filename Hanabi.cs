using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hanabi
{
    using CardRankKnowledge = CardKnowledges<CardRank, int>;
    using CardColorKnowledge = CardKnowledges<CardColor, CardColors>;

    #region User input information
    public interface IUserInput
    {
        string GetInput();
    }

    public class UserConsoleInput : IUserInput
    {
        public string GetInput()
        {
            return Console.ReadLine();
        }
    }
    #endregion

    #region Program output information
    public interface IProgramOutput
    {
        void WriteOutput(string args);
    }

    public class ProgramConsoleOutput : IProgramOutput
    {
        public void WriteOutput(string args)
        {
            Console.WriteLine(args);
        }
    }
    #endregion

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
        public CardRankKnowledge RankKnowledge { get; private set; }
        public CardColorKnowledge ColorKnowledge { get; private set; }

        public CardKnowledge()
        {
            CardNumber = 0;
            RankKnowledge = new CardRankKnowledge();
            ColorKnowledge = new CardColorKnowledge();
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

        public void AddNew(int cardNumber, CardRankKnowledge rankKnowledge, CardColorKnowledge colorKnowledge)
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

    public class GameEngine
    {
        public Player[] Players { get; private set; }
        private Player _currentPlayer;
        private const int PlayersCount = 2;
        private GameField _gameField;
        private int _playerTurn;
        private int _usualTurnNumber;
        private int _riskTurnNumber;
        private int _cardsPlayedCorrect;
        private const int CardsPerPlayer = 5;
        private bool _wrongMove;
        private bool _skipToNextGame;

        public GameEngine()
        {
            InitializeNewGame();
        }

        #region Game start
        private void InitializeNewGame()
        {
            Players = new Player[PlayersCount];
            for (int playerNumber = 0; playerNumber < PlayersCount; playerNumber++)
            {
                Players[playerNumber] = new Player();
            }
            _playerTurn = 1;
            _currentPlayer = Players[_playerTurn];
            _usualTurnNumber = 0;
            _riskTurnNumber = 0;
            _cardsPlayedCorrect = 0;
            _wrongMove = false;
            _skipToNextGame = false;
        }

        private void DealCards()
        {
            for (var playerNumber = 0; playerNumber < PlayersCount; playerNumber++)
                for (var cardCounter = 0; cardCounter < CardsPerPlayer; cardCounter++)
                    Players[playerNumber].AddCard(_gameField.GetCardFromDeck());

            for (var playerNumber = 0; playerNumber < PlayersCount; playerNumber++)
                Players[playerNumber].AddTeamMateInformation(Players[(playerNumber + 1) % PlayersCount]);
        }

        public void StartGame(IUserInput userInput, IProgramOutput programOutput)
        {
            while (true)
            {
                var userCommand = ReadUserCommand(userInput);
                if (userCommand == null)
                    break;
                var commandPattern = GetCommandPattern(userCommand);
                if(_skipToNextGame && commandPattern != Commands.StartNewGame)
                    continue;
                var args = GetCommandArgs(userCommand, commandPattern);
                MakeMove(commandPattern, args);
                SwitchPlayer();
                if (IsGameOver())
                {
                    WriteOutputInformation(programOutput);
                    _skipToNextGame = true;
                }
            }
        }

        private void RestartGame(string args)
        {
            _gameField = new GameField(args);
            InitializeNewGame();
            DealCards();
        }

        private string ReadUserCommand(IUserInput userInput)
        {
            var userCommand = userInput.GetInput();
            return userCommand == null ? null : userCommand.ToLower();
        }
        #endregion

        #region Movements
        private void MakeMove(string commandName, string args)
        {
            _usualTurnNumber++;
            switch (commandName)
            {
                case Commands.StartNewGame:
                    RestartGame(args);
                    break;
                case Commands.PlayCard:
                    PlayCard(args);
                    break;
                case Commands.DropCard:
                    DropCard(args);
                    break;
                case Commands.TellColor:
                    TellColor(args);
                    break;
                case Commands.TellRank:
                    TellRank(args);
                    break;
            }
        }

        private void PlayCard(string args)
        {
            var cardNumber = GetCardNumberFromArgumets(args);
            if (CheckPlayCardAccuracy(cardNumber))
            {
                var riskyMove = IsRiskyMove(cardNumber);
                _currentPlayer.PlayCard(cardNumber, _gameField);
                if (riskyMove)
                    _riskTurnNumber++;
                _cardsPlayedCorrect++;
            }
            else
            {
                _wrongMove = true;
                _currentPlayer.DropCard(cardNumber, _gameField);
            }
        }

        private void DropCard(string args)
        {
            _currentPlayer.DropCard(GetCardNumberFromArgumets(args), _gameField);
        }

        private void TellColor(string args)
        {
            var color = new CardColor();
            TellTeamMate(color, args);
        }

        private void TellRank(string args)
        {
            var rank = new CardRank();
            TellTeamMate(rank, args);
        }

        private void TellTeamMate<T>(CardInfo<T> parameter, string args)
        {
            int[] cardNumbers;
            ParseTellArguments(args, parameter, out cardNumbers);
            if (CheckTellAccuracy(parameter, cardNumbers))
            {
                _currentPlayer.TeamMate.ListenTeamMate(parameter, cardNumbers);
            }
            else
                _wrongMove = true;
        }

        private void SwitchPlayer()
        {
            _playerTurn = (_playerTurn + 1) % PlayersCount;
            _currentPlayer = Players[_playerTurn];
        }
        #endregion

        #region Parsers
        private int GetCardNumberFromArgumets(string args)
        {
            int cardNumber;
            if (!int.TryParse(args, out cardNumber)
                || cardNumber < 0 || cardNumber > _currentPlayer.MyCards.Count)
                throw new ArgumentException();
            return cardNumber;
        }

        private string GetCommandPattern(string userCommand)
        {
            var allCommands = typeof(Commands).GetFields().ToList();
            var commandPattern = allCommands
                .Where(command => Regex.IsMatch(userCommand, command.GetValue(null).ToString().ToLower()))
                .Select(command => command.GetValue(null).ToString()).FirstOrDefault();
            if (commandPattern == null)
                throw new ArgumentException();
            return commandPattern;
        }

        public string GetCommandArgs(string userCommand, string patternCommand)
        {
            var commandArgs = Regex.Matches(userCommand.ToLower(), patternCommand.ToLower());
            var result = "";
            foreach (Match card in commandArgs)
            {
                result += card.Groups[1].Value;
                result += " " + card.Groups[2].Value;
            }
            return result;
        }

        private void ParseTellArguments<T>(string args, CardInfo<T> cardInfo, out int[] cardNumbers)
        {
            var arguments = args.Split(' ');
            cardInfo.SetInfoByString(arguments[0]);
            var cards = arguments.Skip(1).ToArray();
            cardNumbers = Array.ConvertAll(cards, int.Parse);
        }
        #endregion

        #region Checks
        private bool CheckTellAccuracy<T>(CardInfo<T> cardInfo, IEnumerable<int> cardNumbers)
        {
            var teamMateCardIndexes = _currentPlayer.TeamMate.MyCards
                    .Select((x, i) => cardInfo.EqualsToCard(x) ? i : -1)
                    .Where(i => i != -1)
                    .ToArray();
            return cardNumbers.SequenceEqual(teamMateCardIndexes);
        }

        private bool CheckPlayCardAccuracy(int cardNumber)
        {
            var card = _currentPlayer.LookAtCard(cardNumber);

            var isCorrectRank = _gameField.TableCards.Cards
                .Where(x => x.GetColor().Equals(card.GetColor()))
                .Where(x => x.GetRank() == card.GetRank() - 1)
                .Select(x => x)
                .FirstOrDefault();
            return (isCorrectRank != null);
        }

        private bool IsRiskyMove(int cardNumber)
        {
            if (_currentPlayer.MyCards.Any(card => card.Knowledge.CardNumber.Equals(cardNumber)))
            {
                var currentCard = _currentPlayer.LookAtCard(cardNumber);
                var color = currentCard.Knowledge.ColorKnowledge.KnownParameterIs;
                var rank = currentCard.Knowledge.RankKnowledge.KnownParameterIs;
                var notColors = currentCard.Knowledge.ColorKnowledge.KnownParameterIsNot;

                //player doesn't know any information about card
                if (color == null && rank == null)
                    return true;

                //player knows everything
                if ((color != null && rank != null) || (rank != null && rank.GetCardInfo() == 1 && _gameField.TableCards.Cards.All(card => card.GetRank() == 0)))
                    return false;

                //player checks suggestions about card's color
                if (rank != null)
                    return CheckHypotheticCardsRisk(GetHypotheticColors(notColors), rank);
            }
            return true;
        }

        private bool CheckHypotheticCardsRisk(List<CardColors> hypotheticColor, CardRank cardRank)
        {
            var rightCardCounter = 0;
            var rightHypothesis = hypotheticColor
                .All(color => _gameField.TableCards.Cards
                    .Select(card => card.GetColor())
                    .Contains(color));

            if (!rightHypothesis)
                return true;
            rightCardCounter += hypotheticColor
                .Select(color => _gameField.TableCards.Cards
                    .Where(card => card.GetColor().Equals(color))
                    .Select(card => card.Rank)
                    .FirstOrDefault())
                .Count(rank => rank.GetCardInfo().Equals(cardRank.GetCardInfo() - 1));
            return rightCardCounter != hypotheticColor.Count;
        }

        private List<CardColors> GetHypotheticColors(IEnumerable<CardColor> notColors)
        {
            var colorType = typeof(CardColors);
            var temp = new CardColor();
            return Enum.GetNames(colorType)
                    .Where(colorName => !notColors
                        .Select(cardColor => cardColor.GetCardInfo())
                        .Contains(temp.GetCardInfoByString(colorName)))
                        .Select(temp.GetCardInfoByString)
                    .ToList();
        }

        private bool IsGameOver()
        {
            return (_gameField.DeckCards.GetCardsCount() == 0
                    || _cardsPlayedCorrect == 25 || _wrongMove);
        }
        #endregion

        #region Show results
        private void WriteOutputInformation(IProgramOutput programOutput)
        {
            var outputArgs = String.Format("Turn: {0}, cards: {1}, with risk: {2}", _usualTurnNumber,
                _cardsPlayedCorrect,
                _riskTurnNumber);
            programOutput.WriteOutput(outputArgs);
        }
        #endregion
    }

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

    class HanabiGame
    {
        static void Main()
        {
            var newGameEngine = new GameEngine();
            newGameEngine.StartGame(new UserConsoleInput(), new ProgramConsoleOutput());
        }
    }
}
