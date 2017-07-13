using System;
using System.Linq;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Repositories;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Commands
{
    [ConsoleCommand("list-commands", "Lists all available commands")]
    public class ListCommands : ConsoleCommandBase
    {
        public override ConsoleResultModel Run()
        {

            try
            {
                var lstOut = CommandRepository.Instance.GetCommands().Values.OrderBy(c => c.Name + '.' + c.Name);
                return new ConsoleResultModel($"Found {lstOut.Count()} commands")
                {
                    Data = lstOut,
                    FieldOrder = new[] {
                    "Name", "Description", "Version", "NameSpace" }
                };
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return new ConsoleErrorResultModel("An error occurred while attempting to restart the application.");
            }
        }
    }
}