using System.Collections.Generic;
using Dnn.PersonaBar.Library.Prompt.Common;

namespace Dnn.PersonaBar.Prompt.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICommandRepository
    {
        SortedDictionary<string, Command> GetCommands();
    }
}
