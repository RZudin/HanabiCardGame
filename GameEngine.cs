using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Hanabi.CardsInfo;
using Hanabi.GameInfo;
using Hanabi.InputOutput;

namespace Hanabi
{
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
                if (_skipToNextGame && commandPattern != Commands.StartNewGame)
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
}
