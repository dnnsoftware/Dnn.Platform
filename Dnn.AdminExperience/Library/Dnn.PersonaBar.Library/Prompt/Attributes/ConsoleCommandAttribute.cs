// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace Dnn.PersonaBar.Library.Prompt.Attributes
{
    [Obsolete("Moved to DotNetNuke.Prompt in the core library project. Will be removed in DNN 11.", false)]
    [AttributeUsage(AttributeTargets.Class)]
#pragma warning disable CS3015 // Type has no accessible constructors which use only CLS-compliant types
    public class ConsoleCommandAttribute : Attribute
#pragma warning restore CS3015 // Type has no accessible constructors which use only CLS-compliant types
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }

        public ConsoleCommandAttribute(string name, string category, string description)
        {
            Name = name;
            Category = category;
            Description = description;
        }
    }
}
