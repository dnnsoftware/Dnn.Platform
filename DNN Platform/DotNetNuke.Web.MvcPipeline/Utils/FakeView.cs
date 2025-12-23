// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Utils
{
    using System.IO;
    using System.Web.Mvc;

    /// <summary>
    /// Fake view implementation for HtmlHelper contexts that don't require actual view rendering.
    /// </summary>
    internal class FakeView : IView
    {
        /// <inheritdoc/>
        public void Render(ViewContext viewContext, TextWriter writer)
        {
            // No-op implementation since we're only using this for HtmlHelper context.
        }
    }
}
