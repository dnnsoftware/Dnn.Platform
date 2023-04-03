// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>Abstraction of Telerik related utilities.</summary>
    public interface ITelerikUtils
    {
        /// <summary>Gets the path of the bin folder.</summary>
        string BinPath { get; }

        /// <summary>Gets an <see cref="IEnumerable{T}"/> containing all assemblies that depend on Telerik.</summary>
        /// <returns>An <see cref="IEnumerable{T}"/> containing all assemblies that depend on Telerik.</returns>
        IEnumerable<string> GetAssembliesThatDependOnTelerik();

        /// <summary>Checks whether Telerik is installed on this site or not.</summary>
        /// <returns><c>True</c> if Telerik is found in this site, or <c>False</c> otherwise.</returns>
        bool TelerikIsInstalled();

        /// <summary>Loads the Telerik assembly and returns its version number.</summary>
        /// <returns>The Telerik version number.</returns>
        /// <exception cref="IOException">
        /// An error occurred while loading the Telerik assembly.
        /// The file may not exist, or it may be locked by another process.
        /// The <see cref="Exception.InnerException"/> property contains the cause of the error.
        /// </exception>
        Version GetTelerikVersion();
    }
}
