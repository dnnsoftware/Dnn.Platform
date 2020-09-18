// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Authentication
{
    using System.Web;

    using DotNetNuke.Common.Internal;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The LogOffHandler class provides a replacement for the LogOff page.
    /// </summary>
    public class LogOffHandler : IHttpHandler
    {
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
    }
}
