using System.Collections.Generic;
using Dnn.PersonaBar.Prompt.Components.Models;

namespace Dnn.PersonaBar.Prompt.Components.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICommandRepository
    {
        SortedDictionary<string, Command> GetCommands();
    }
}
