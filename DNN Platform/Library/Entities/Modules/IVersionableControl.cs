// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules
{
    /// <summary>
    /// This interface is implemented by module control to let the page tell to the control what
    /// content version it should be render.
    /// </summary>
    public interface IVersionableControl
    {
        /// <summary>
        /// Indicate to the module control what content version need to be shown on rendering.
        /// </summary>
        /// <param name="version">Version number.</param>
        void SetModuleVersion(int version);
    }
}
