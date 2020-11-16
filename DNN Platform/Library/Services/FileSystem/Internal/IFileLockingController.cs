// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    public interface IFileLockingController
    {
        /// <summary>
        /// Checks if a file is locked or not.
        /// </summary>
        /// <param name="file">The file to be checked.</param>
        /// <param name="lockReasonKey">The friendly reason why the file is locked.</param>
        /// <returns>True if the file is locked, false in otherwise.</returns>
        bool IsFileLocked(IFileInfo file, out string lockReasonKey);

        /// <summary>
        /// Checks if the file is out of Publish Period.
        /// </summary>
        /// <param name="file">the file to be checked.</param>
        /// <param name="portalId">The Portal Id where the file is contained.</param>
        /// <param name="userId">The user Id who is accessing to the file.</param>
        /// <returns>True if the file is out of publish period, false in otherwise. In anycase, True for admin or host users.</returns>
        bool IsFileOutOfPublishPeriod(IFileInfo file, int portalId, int userId);
    }
}
