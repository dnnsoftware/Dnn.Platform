using System.Collections.Generic;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Prompt.Components.Models;

namespace Dnn.PersonaBar.Prompt.Components.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICommandRepository
    {
        SortedDictionary<string, Command> GetCommands();
        CommandHelp GetCommandHelp(CommandInputModel command, IConsoleCommand consoleCommand, bool showSyntax = false, bool showLearn = false);
    }
}
