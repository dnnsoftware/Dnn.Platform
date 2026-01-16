// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    /// <summary>A contract specifying the ability to expose information about an extension point tied to a user control.</summary>
    public interface IUserControlExtensionPoint : IExtensionPoint
    {
        /// <summary>Gets the path to the user control's source file.</summary>
        string UserControlSrc { get; }

        /// <summary>Gets a value indicating whether the extension point is visible.</summary>
        bool Visible { get; }
    }
}
