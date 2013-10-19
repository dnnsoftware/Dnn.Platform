#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Workflow;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;

namespace DotNetNuke.Services.FileSystem.Internal
{
    public class FileLockingController : ServiceLocator<IFileLockingController, FileLockingController>, IFileLockingController
    {
        public bool IsFileLocked(IFileInfo file, out string lockReasonKey)
        {
            lockReasonKey = "";

            var allowedUser = UserSecurityController.Instance.IsHostAdminUser(file.PortalId);
            if (allowedUser)
            {
                return false;
            }

            if (file.ContentItemID != Null.NullInteger)
            {
                var workflowCompleted = ContentWorkflowController.Instance.IsWorkflowCompleted(file.ContentItemID);
                if (!workflowCompleted)
                {
                    lockReasonKey = "FileLockedRunningWorkflowError";
                    return true;
                }
            }

            var outOfPublishPeriod = IsFileOutOfPublishPeriod(file);
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
            return IsFileOutOfPublishPeriod(file);
        }

        private bool IsFileOutOfPublishPeriod(IFileInfo file)
        {
            //Publish Period locks
            return (file.EnablePublishPeriod && (file.StartDate > DateTime.Today || (file.EndDate < DateTime.Today && file.EndDate != Null.NullDate)));
        }

        protected override Func<IFileLockingController> GetFactory()
        {
            return () => new FileLockingController();
        }
    }
}
