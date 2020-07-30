// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    /// <summary>
    /// A contract specifying the ability for a module's business controller
    /// class to run custom upgrade logic after the module has been installed.
    /// </summary>
    public interface IUpgradeable
    {
        /// <summary>Runs custom upgrade logic for a module.</summary>
        /// <param name="Version">The version the module is being upgraded to.</param>
        /// <returns>A status message.</returns>
        string UpgradeModule(string Version);
    }
}
