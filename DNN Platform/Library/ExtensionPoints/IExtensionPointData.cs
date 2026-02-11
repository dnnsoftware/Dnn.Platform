// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    public interface IExtensionPointData
    {
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
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
