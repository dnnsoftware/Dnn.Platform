namespace Dnn.ExportImport.Components.Common
{
    public enum JobType
    {
        Export = 0, // never change these numbers
        Import
    }

    public enum JobStatus
    {
        Submitted = 0, // never change these numbers
        InProgress,
        Successful,
        Failed,
        Cancelled
    }

    /// <summary>
    /// Mode for export job.
    /// </summary>
    public enum ExportMode
    {
        Full = 0,
        Differential = 1
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

    public enum ReportLevel
    {
        Verbose,
        Info,
        Warn,
        Error
    }
}
