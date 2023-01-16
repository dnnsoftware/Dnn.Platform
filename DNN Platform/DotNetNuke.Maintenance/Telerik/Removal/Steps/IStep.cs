// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    /// <summary>An interface that represent a process step that can be executed.</summary>
    internal interface IStep
    {
        /// <summary>Gets or sets the step name.</summary>
        string Name { get; set; }

        /// <summary>Gets the remarks collected during step execution.</summary>
        string Notes { get; }

        /// <summary>Gets a value indicating whether successful executions are logged to the progress report or not.</summary>
        /// <remarks>Failed executions are always logged.</remarks>
        bool Quiet { get; }

        /// <summary>Gets a value indicating whether the step succeeded, failed or was not executed.</summary>
        bool? Success { get; }

        /// <summary>Executes the step.</summary>
        void Execute();
    }
}
