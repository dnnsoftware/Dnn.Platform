// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Utils
{
    using System.Web.Mvc;

    /// <summary>
    /// Simple <see cref="IViewDataContainer"/> implementation used for HtmlHelper contexts in <see cref="MvcViewEngine"/>.
    /// </summary>
    internal class ViewDataContainer : IViewDataContainer
    {
        /// <inheritdoc/>
        public ViewDataDictionary ViewData { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewDataContainer"/> class.
        /// </summary>
        /// <param name="viewData">The view data dictionary.</param>
        public ViewDataContainer(ViewDataDictionary viewData)
        {
            this.ViewData = viewData;
        }
    }
}
