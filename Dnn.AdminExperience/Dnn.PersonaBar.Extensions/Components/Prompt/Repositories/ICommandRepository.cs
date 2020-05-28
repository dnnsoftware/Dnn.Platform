// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Prompt.Components.Models;

namespace Dnn.PersonaBar.Prompt.Components.Repositories
{
    [Obsolete("Moved to DotNetNuke.Abstractions.Prompt in the core abstractions project. Will be removed in DNN 11.", false)]
    public interface ICommandRepository
    {
        SortedDictionary<string, Command> GetCommands();
        CommandHelp GetCommandHelp(string[] args, IConsoleCommand consoleCommand);
    }
}
