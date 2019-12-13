// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.Services.Upgrade.Internals.Steps
{
    /// <summary>
    /// Status of an Installation Step
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
        AppRestart = 5
    }
}
