// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Web;

using DotNetNuke.Common.Internal;

#endregion

namespace DotNetNuke.Services.Authentication
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The LogOffHandler class provides a replacement for the LogOff page
    /// </summary>
    public class LogOffHandler : IHttpHandler
    {
        #region IHttpHandler Members

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Redirect(TestableGlobals.Instance.NavigateURL("LogOff"));
        }

        #endregion
    }
}
