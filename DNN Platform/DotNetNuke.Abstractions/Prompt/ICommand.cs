// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Abstractions.Prompt
{
    public interface ICommand
    {
        string Category { get; set; }
        Type CommandType { get; set; }
        string Description { get; set; }
        string Key { get; set; }
        string Name { get; set; }
        string Version { get; set; }
    }
}
