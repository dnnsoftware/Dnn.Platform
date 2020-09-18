// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Repositories
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Prompt.Components.Models;
    [Obsolete("Moved to DotNetNuke.Abstractions.Prompt in the core abstractions project. Will be removed in DNN 11.", false)]
    public interface ICommandRepository
    {
        SortedDictionary<string, Command> GetCommands();
        CommandHelp GetCommandHelp(string[] args, IConsoleCommand consoleCommand);
    }
}
