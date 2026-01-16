// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    /// <summary>A contract specifying the ability to expose information about an extension point.</summary>
    public interface IExtensionPoint
    {
        /// <summary>Gets the text.</summary>
        string Text { get; }

        /// <summary>Gets the icon.</summary>
        string Icon { get; }

        /// <summary>Gets the order.</summary>
        int Order { get; }
    }
}
