using Hanabi.InputOutput;

namespace Hanabi
{
    class HanabiGame
    {
        static void Main()
        {
            var newGameEngine = new GameEngine();
            newGameEngine.StartGame(new UserConsoleInput(), new ProgramConsoleOutput());
        }
    }
}
