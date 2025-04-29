// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Prompt.Components.Repositories;

using System.Collections.Generic;

using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Prompt.Components.Models;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(9, 7, 0, "Moved to DotNetNuke.Abstractions.Prompt in the core abstractions project")]
public partial interface ICommandRepository
{
    SortedDictionary<string, Command> GetCommands();

    CommandHelp GetCommandHelp(string[] args, IConsoleCommand consoleCommand);
}
