using System;

namespace Hanabi.InputOutput
{

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
}
