// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Prompt.Components.Commands.Commands
{
    using System;
    using System.Globalization;
    using System.Linq;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Prompt.Components.Models;
    using Dnn.PersonaBar.Prompt.Components.Repositories;
    using DotNetNuke.Instrumentation;

    [ConsoleCommand("list-commands", Constants.GeneralCategory, "Prompt_ListCommands_Description")]
    public class ListCommands : ConsoleCommandBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ListCommands));

        /// <inheritdoc/>
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        /// <inheritdoc/>
        public override ConsoleResultModel Run()
        {
            try
            {
                var lstNewCommands = DotNetNuke.Prompt.CommandRepository.Instance.GetCommands().Select(c => new Command
                {
                    Name = c.Name,
                    Category = c.Category,
                    Description = c.Description,
                    Key = c.Key,
                    Version = c.Version,
                });
                var lstOut = CommandRepository.Instance.GetCommands().Values.Concat(lstNewCommands).OrderBy(c => c.Name + '.' + c.Name).ToList();
                return new ConsoleResultModel(string.Format(CultureInfo.InvariantCulture, this.LocalizeString("Prompt_ListCommands_Found"), lstOut.Count))
                {
                    Records = lstOut.Count,
                    Data = lstOut,
                    FieldOrder = ["Name", "Description", "Version", "Category",],
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_ListCommands_Error"));
            }
        }
    }
}
