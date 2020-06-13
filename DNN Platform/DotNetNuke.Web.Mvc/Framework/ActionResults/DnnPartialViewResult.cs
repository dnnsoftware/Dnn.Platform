// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.ActionResults
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using DotNetNuke.Common;

    public class DnnPartialViewResult : PartialViewResult, IDnnViewResult
    {
        public void ExecuteResult(ControllerContext context, TextWriter writer)
        {
            Requires.NotNull("context", context);
            Requires.NotNull("writer", writer);

            if (string.IsNullOrEmpty(this.ViewName))
            {
                this.ViewName = context.RouteData.GetRequiredString("action");
            }

            ViewEngineResult result = null;

            if (this.View == null)
            {
                result = this.ViewEngineCollection.FindPartialView(context, this.ViewName);
                this.View = result.View;
            }

            var viewContext = new ViewContext(context, this.View, this.ViewData, this.TempData, writer);
            this.View.Render(viewContext, writer);

            if (result != null)
            {
                result.ViewEngine.ReleaseView(context, this.View);
            }
        }
    }
}
