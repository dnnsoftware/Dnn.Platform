// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;

namespace DotNetNuke.Abstractions.Prompt
{
    public interface ICommandHelp
    {
        string Description { get; set; }
        string Error { get; set; }
        string Name { get; set; }
        IEnumerable<ICommandOption> Options { get; set; }
        string ResultHtml { get; set; }
    }
}
