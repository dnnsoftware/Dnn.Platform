// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.Internals.Steps
{
    using System;

    /// <summary>
    /// Status of an Installation Step.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public enum StepStatus
    {
        /// <summary>
        /// Step not Started yet.
        /// </summary>
        NotStarted = 0,

        /// <summary>
        /// Step is running.
        /// </summary>
        Running = 1,

        /// <summary>
        /// Step is done and was successful.
        /// </summary>
        Done = 2,

        /// <summary>
        /// Step failed. Retry the existing step.
        /// </summary>
        Retry = 3,

        /// <summary>
        /// Step failed. Abort the next step.
        /// </summary>
        Abort = 4,

        /// <summary>
        /// Step resulted in Application Restart. You should redirect to the same page.
        /// </summary>
        AppRestart = 5,
    }
}
