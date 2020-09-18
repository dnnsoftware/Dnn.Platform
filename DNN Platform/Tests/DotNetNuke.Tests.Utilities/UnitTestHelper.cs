// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Instance.Utilities
{
    using System;
    using System.IO;
    using System.Web;

    public static class UnitTestHelper
    {
        /// <summary>
        /// Sets the HTTP context with a valid simulated request.
        /// </summary>
        /// <param name="host">Host.</param>
        /// <param name="application">Application.</param>
        /// <param name="appPhysicalDir"></param>
        /// <param name="pageName"></param>
        public static void SetHttpContextWithSimulatedRequest(string host, string application, string appPhysicalDir, string pageName)
        {
            const string appVirtualDir = "/";
            var page = application.Replace("/", string.Empty) + "/" + pageName;
            var query = string.Empty;
            TextWriter output = null;

            var workerRequest = new SimulatedHttpRequest(appVirtualDir, appPhysicalDir, page, query, output, host);
            HttpContext.Current = new HttpContext(workerRequest);
        }

        public static void ClearHttpContext()
        {
            HttpContext.Current = null;
        }
    }
}
