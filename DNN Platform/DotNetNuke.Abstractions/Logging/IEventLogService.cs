// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Logging
{
    using System.Collections.Generic;

    /// <summary>
    /// The event log service provides APIs for
    /// managing existing Event Logs in the database.
    /// </summary>
    public interface IEventLogService : IEventLogger
    {
        /// <summary>
        /// Clears the Event Log.
        /// </summary>
        void ClearLog();

        /// <summary>
        /// Deletes a specific Event Log.
        /// </summary>
        /// <param name="logInfo">the log info.</param>
        void DeleteLog(ILogInfo logInfo);

        /// <summary>
        /// Get the Event Log.
        /// </summary>
        /// <param name="portalID">The portal id.</param>
        /// <param name="logType">The log type.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="totalRecords">The total records.</param>
        /// <returns>The logs.</returns>
        IEnumerable<ILogInfo> GetLogs(int portalID, string logType, int pageSize, int pageIndex, ref int totalRecords);

        /// <summary>
        /// Get a single log.
        /// </summary>
        /// <param name="log">The log to retrieve.</param>
        /// <param name="returnType">The return type.</param>
        /// <returns>The single log.</returns>
        object GetSingleLog(ILogInfo log, LoggingProviderReturnType returnType);

        /// <summary>
        /// Purge the log buffer.
        /// </summary>
        void PurgeLogBuffer();
    }
}
