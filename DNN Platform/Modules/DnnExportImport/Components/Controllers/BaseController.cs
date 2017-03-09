#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
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

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Log.EventLog;

namespace Dnn.ExportImport.Components.Controllers
{
    public abstract class BaseController
    {
        protected void AddEventLog(int portalId, int userId, int jobId, string logTypeKey)
        {
            var objSecurity = new PortalSecurity();
            var portalInfo = PortalController.Instance.GetPortal(portalId);
            var userInfo = UserController.Instance.GetUser(portalId, userId);
            var username = objSecurity.InputFilter(userInfo.Username,
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);

            var log = new LogInfo
            {
                LogTypeKey = logTypeKey,
                LogPortalID = portalId,
                LogPortalName = portalInfo.PortalName,
                LogUserName = username,
                LogUserID = userId,
            };

            log.AddProperty("JobID", jobId.ToString());
            LogController.Instance.AddLog(log);
        }
    }
}