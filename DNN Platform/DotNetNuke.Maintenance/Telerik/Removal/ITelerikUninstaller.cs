// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Removal
{
    using System.Collections.Generic;

    /// <summary>Handles Telerik uninstall process.</summary>
    public interface ITelerikUninstaller
    {
        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of type <see cref="UninstallSummaryItem"/>
        /// containing the overall execution progress.
        /// </summary>
        IEnumerable<UninstallSummaryItem> Progress { get; }

        /// <summary>Executes the Telerik uninstall process.</summary>
        void Execute();
    }
}
