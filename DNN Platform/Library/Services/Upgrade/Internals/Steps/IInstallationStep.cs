// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.Internals.Steps
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>This event gets fired when any activity gets recorded.</summary>
    /// <param name="status">The details text.</param>
    [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
    public delegate void ActivityEventHandler(string status);

    /// <summary>Interface for an Installation Step.</summary>
    public interface IInstallationStep
    {
        /// <summary>This event gets fired when any activity gets recorded</summary>
        event ActivityEventHandler Activity;

        /// <summary>Gets any details of the task while it's executing.</summary>
        string Details { get; }

        /// <summary>Gets percentage done.</summary>
        int Percentage { get; }

        /// <summary>Gets step Status.</summary>
        StepStatus Status { get; }

        /// <summary>Gets list of Errors.</summary>
        IList<string> Errors { get; }

        /// <summary>Main method to execute the step.</summary>
        void Execute();
    }
}
