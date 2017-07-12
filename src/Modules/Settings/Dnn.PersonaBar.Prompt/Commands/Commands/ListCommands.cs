using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Repositories;
using System;
using System.Linq;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Models;
namespace Dnn.PersonaBar.Prompt.Commands.Commands
{
    [ConsoleCommand("list-commands", "Lists all available commands", new string[] { })]
    public class ListCommands : ConsoleCommandBase
    {
        public override ConsoleResultModel Run()
        {

            try
            {
                var lstOut = CommandRepository.Instance.GetCommands().Values.OrderBy(c => c.Name + '.' + c.Name);
                return new ConsoleResultModel(string.Format("Found {0} commands", lstOut.Count()))
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