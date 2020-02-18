// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Utilities
{
    [ConsoleCommand("echo", Constants.GeneralCategory, "Prompt_Echo_Description")]
    public class Echo : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        public override ConsoleResultModel Run()
        {
            if (Args.Length > 1)
            {
                return new ConsoleResultModel(Args[1]);
            }


            return new ConsoleErrorResultModel(LocalizeString("Prompt_Echo_Nothing"));
        }

    }
}
