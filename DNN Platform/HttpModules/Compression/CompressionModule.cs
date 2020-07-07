// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Compression
{
    using System;
    using System.IO;
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;

    [Obsolete("The http modules in web.config removed in 9.2.0, this class need to be removed in future release for upgrade compactible.. Scheduled removal in v11.0.0.")]
    public class CompressionModule : IHttpModule
    {
        /// <summary>
        /// Init the handler and fulfill <see cref="IHttpModule"/>.
        /// </summary>
        /// <remarks>
        /// This implementation hooks the ReleaseRequestState and PreSendRequestHeaders events to
        /// figure out as late as possible if we should install the filter.  Previous versions did
        /// not do this as well.
        /// </remarks>
        /// <param name="context">The <see cref="HttpApplication"/> this handler is working for.</param>
        public void Init(HttpApplication context)
        {
        }

        /// <summary>
        /// Implementation of <see cref="IHttpModule"/>.
        /// </summary>
        /// <remarks>
        /// Currently empty.  Nothing to really do, as I have no member variables.
        /// </remarks>
        public void Dispose()
        {
        }
    }
}
