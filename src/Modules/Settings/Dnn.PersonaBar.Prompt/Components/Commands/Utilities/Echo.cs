using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Utilities
{
    [ConsoleCommand("echo", "Echos back the first argument received")]
    public class Echo : ConsoleCommandBase
    {
        public override ConsoleResultModel Run()
        {
            if (Args.Length > 1)
            {
                return new ConsoleResultModel(Args[1]);
            }


            return new ConsoleErrorResultModel("Nothing to echo back");
        }

    }
}