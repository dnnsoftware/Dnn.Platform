// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    using System;
    using System.Linq;

    using DotNetNuke.Entities.Modules.Settings;

    public class ComplexTypeSerializer : ISettingsSerializer<ComplexType>
    {
        public string Serialize(ComplexType value)
        {
            return $"{value.X} | {value.Y}";
        }

        public ComplexType Deserialize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new ComplexType(5, 10);
            }

            var values = value.Split(new[] { '|', }, StringSplitOptions.RemoveEmptyEntries).Select(val => val.Trim()).Select(int.Parse).ToArray();
            return new ComplexType(values[0], values[1]);
        }
    }
}
