// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
