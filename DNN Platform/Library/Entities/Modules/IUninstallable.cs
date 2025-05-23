// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    /// <summary>
    /// A contract specifying the ability for a module's business controller
    /// class to run custom uninstall logic when the module is being uninstalled.
    /// </summary>
    public interface IUninstallable
    {
        /// <summary>Runs custom uninstall logic for a module.</summary>
        /// <param name="deleteFiles">A flag indicating whether the user chose to delete files during uninstall.</param>
        /// <returns>A status message.</returns>
        string UninstallModule(bool deleteFiles);
    }
}