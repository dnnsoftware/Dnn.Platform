// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Common
{
    /// <summary>
    /// Defines the possible job types.
    /// </summary>
    public enum JobType
    {
        /// <summary>
        /// An export job.
        /// </summary>
        Export = 0, // never change these numbers

        /// <summary>
        /// An import job.
        /// </summary>
        Import,
    }

    /// <summary>
    /// Defines the possible job statuses.
    /// </summary>
    public enum JobStatus
    {
        /// <summary>
        /// The job was submitted but not yet started.
        /// </summary>
        Submitted = 0, // never change these numbers

        /// <summary>
        /// The job is currently in progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// The job completed successfully.
        /// </summary>
        Successful,

        /// <summary>
        /// The job completed but failed.
        /// </summary>
        Failed,

        /// <summary>
        /// The job was cancelled.
        /// </summary>
        Cancelled,
    }

    /// <summary>
    /// Mode for export job.
    /// </summary>
    public enum ExportMode
    {
        /// <summary>
        /// Exports the full content.
        /// </summary>
        Full = 0,

        /// <summary>
        /// Exports the difference only.
        /// </summary>
        Differential = 1,
    }

    /// <summary>
    /// Specifies what to do when there is a collision during the import process.
    /// </summary>
    public enum CollisionResolution
    {
        /// <summary>
        /// Ignore the imported item and continue.
        /// </summary>
        Ignore,

        /// <summary>
        /// Overwrites the existing item upon importing.
        /// </summary>
        Overwrite,
    }

    /// <summary>
    /// Defines the possible report (log) levels.
    /// </summary>
    public enum ReportLevel
    {
        /// <summary>
        /// Reports everything.
        /// </summary>
        Verbose,

        /// <summary>
        /// Reports information, warnings and errors.
        /// </summary>
        Info,

        /// <summary>
        /// Reports warnings and errors.
        /// </summary>
        Warn,

        /// <summary>
        /// Reports errors only.
        /// </summary>
        Error,
    }
}
