// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.ComponentModel;

namespace DotNetNuke.ExtensionPoints
{
    public interface IExtensionPointData
    {
        string Module { get; }

        string Name { get; }

        string Group { get; }

        int Priority { get; }

        [DefaultValue(false)]
        bool DisableOnHost { get; }

        [DefaultValue(false)]
        bool DisableUnauthenticated { get; }
    }
}
