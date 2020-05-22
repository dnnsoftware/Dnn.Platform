// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;

namespace DotNetNuke.Abstractions.Prompt
{
    public interface ICommandRepository
    {
        SortedDictionary<string, ICommand> GetCommands();
        ICommandHelp GetCommandHelp(IConsoleCommand consoleCommand);
    }
}
