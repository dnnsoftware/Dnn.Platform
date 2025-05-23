﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    /// <summary>A contract specifying the ability to import and export the content of a module.</summary>
    public interface IPortable
    {
        /// <summary>Exports the content of this module.</summary>
        /// <param name="moduleID">The ID of the module to export.</param>
        /// <returns>The module's content serialized as a <see cref="string"/>.</returns>
        string ExportModule(int moduleID);

        /// <summary>Imports the content of a module.</summary>
        /// <param name="moduleID">The ID of the module into which the content is being imported.</param>
        /// <param name="content">The content to import.</param>
        /// <param name="version">The version of the module from which the content is coming.</param>
        /// <param name="userID">The ID of the user performing the import.</param>
        void ImportModule(int moduleID, string content, string version, int userID);
    }
}
