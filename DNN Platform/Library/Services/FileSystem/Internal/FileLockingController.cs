// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;

    public class FileLockingController : ServiceLocator<IFileLockingController, FileLockingController>, IFileLockingController
    {
        public bool IsFileLocked(IFileInfo file, out string lockReasonKey)
        {
            lockReasonKey = string.Empty;

            var allowedUser = UserSecurityController.Instance.IsHostAdminUser(file.PortalId);
            if (allowedUser)
            {
                return false;
            }

            if (file.ContentItemID != Null.NullInteger)
            {
                var workflowCompleted = WorkflowEngine.Instance.IsWorkflowCompleted(file.ContentItemID);
                if (!workflowCompleted)
                {
                    lockReasonKey = "FileLockedRunningWorkflowError";
                    return true;
                }
            }

            var outOfPublishPeriod = this.IsFileOutOfPublishPeriod(file);
            if (outOfPublishPeriod)
            {
                lockReasonKey = "FileLockedOutOfPublishPeriodError";
                return true;
            }

            return false;
        }

        public bool IsFileOutOfPublishPeriod(IFileInfo file, int portalId, int userId)
        {
            if (UserSecurityController.Instance.IsHostAdminUser(portalId, userId))
            {
                return false;
            }

            return this.IsFileOutOfPublishPeriod(file);
        }

        protected override Func<IFileLockingController> GetFactory()
        {
            return () => new FileLockingController();
        }

        private bool IsFileOutOfPublishPeriod(IFileInfo file)
        {
            // Publish Period locks
            return file.EnablePublishPeriod && (file.StartDate > DateTime.Today || (file.EndDate < DateTime.Today && file.EndDate != Null.NullDate));
        }
    }
}
