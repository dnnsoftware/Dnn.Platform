// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Services.Upgrade.Internals.Steps
{

    /// <summary>
    /// This event gets fired when any activity gets recorded
    /// </summary>
    public delegate void ActivityEventHandler(string status);

    /// <summary>
    /// Interface for an Installation Step
    /// </summary>
    /// -----------------------------------------------------------------------------
    public interface IInstallationStep
    {
        #region Properties

        /// <summary>
        /// Any details of the task while it's executing
        /// </summary>
        string Details { get; }

        /// <summary>
        /// Percentage done
        /// </summary>
        int Percentage { get; }

        /// <summary>
        /// Step Status
        /// </summary>
        StepStatus Status { get; }

        /// <summary>
        /// List of Errors
        /// </summary>
        IList<string> Errors { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Main method to execute the step
        /// </summary>
        void Execute();

        #endregion

        #region Events

        /// <summary>
        /// This event gets fired when any activity gets recorded
        /// </summary>
        event ActivityEventHandler Activity;

        #endregion

    }
}
