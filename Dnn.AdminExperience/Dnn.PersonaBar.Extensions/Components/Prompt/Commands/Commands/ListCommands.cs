﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Linq;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using Dnn.PersonaBar.Prompt.Components.Repositories;
using DotNetNuke.Instrumentation;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Commands
{
    [ConsoleCommand("list-commands", Constants.GeneralCategory, "Prompt_ListCommands_Description")]
    public class ListCommands : ConsoleCommandBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ListCommands));
        public override string LocalResourceFile => Constants.LocalResourcesFile;

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
                    Version = c.Version
                });
                var lstOut = CommandRepository.Instance.GetCommands().Values.Concat(lstNewCommands).OrderBy(c => c.Name + '.' + c.Name).ToList();
                return new ConsoleResultModel(string.Format(LocalizeString("Prompt_ListCommands_Found"), lstOut.Count))
                {
                    Records = lstOut.Count,
                    Data = lstOut,
                    FieldOrder = new[] {
                    "Name", "Description", "Version", "Category" }
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(LocalizeString("Prompt_ListCommands_Error"));
            }
        }
    }
}
