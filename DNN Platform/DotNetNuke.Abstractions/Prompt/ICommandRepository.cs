using System.Collections.Generic;

namespace DotNetNuke.Abstractions.Prompt
{
    public interface ICommandRepository
    {
        SortedDictionary<string, ICommand> GetCommands();
        ICommandHelp GetCommandHelp(IConsoleCommand consoleCommand);
    }
}
