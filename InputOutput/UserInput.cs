using System;

namespace Hanabi.InputOutput
{
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
}
