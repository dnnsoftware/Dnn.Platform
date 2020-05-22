// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Abstractions.Prompt
{
    public interface ICommandOption
    {
        string DefaultValue { get; set; }
        string DescriptionKey { get; set; }
        string Name { get; set; }
        bool Required { get; set; }
    }
}
