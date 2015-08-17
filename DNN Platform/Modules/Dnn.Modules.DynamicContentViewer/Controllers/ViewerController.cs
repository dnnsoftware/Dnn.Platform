// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Mvc;
using DotNetNuke.Web.Mvc.Framework.Controllers;

namespace Dnn.Modules.DynamicContentViewer.Controllers
{
    public class ViewerController : DnnController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
