// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Dnn.Modules.DynamicContentViewer.ViewEngines;

namespace Dnn.Modules.DynamicContentViewer
{
    /// <summary>
    /// 
    /// </summary>
    public class ModuleApplication : DotNetNuke.Web.Mvc.Framework.Modules.ModuleApplication
    {
        /// <summary>
        /// 
        /// </summary>
        public ModuleApplication()
        {
            DefaultActionName = "Index";
            DefaultControllerName = "Viewer";
            DefaultNamespaces = new [] { "Dnn.Modules.DynamicContentViewer.Controllers"};
            ModuleName = "Dnn.DynamicContentViewer";
            FolderPath = "Dnn/DynamicContentViewer";
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void Init()
        {
            ViewEngines.Add(new DynamicContentViewEngine());

            base.Init();
        }
    }
}
