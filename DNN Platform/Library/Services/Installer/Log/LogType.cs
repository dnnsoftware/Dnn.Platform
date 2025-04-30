// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer.Log
{
    public enum LogType
    {
        /// <summary>An information log.</summary>
        Info = 0,

        /// <summary>A warning log.</summary>
        Warning = 1,

        /// <summary>A failure log.</summary>
        Failure = 2,

        /// <summary>The beginning of a task.</summary>
        StartJob = 3,

        /// <summary>The end of a task.</summary>
        EndJob = 4,
    }
}
