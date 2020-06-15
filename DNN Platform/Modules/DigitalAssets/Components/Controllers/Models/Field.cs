// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models
{
    using System;

    public class Field
    {
        public Field(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }

        public string DisplayName { get; set; }

        public object Value { get; set; }

        public string StringValue { get; set; }

        public Type Type { get; set; }
    }
}
