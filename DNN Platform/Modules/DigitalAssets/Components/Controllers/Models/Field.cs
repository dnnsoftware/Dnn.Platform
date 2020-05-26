// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models
{
    public class Field
    {
        public Field(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public string DisplayName { get; set; }

        public object Value { get; set; }

        public string StringValue { get; set; }

        public Type Type { get; set; }
    }
}
