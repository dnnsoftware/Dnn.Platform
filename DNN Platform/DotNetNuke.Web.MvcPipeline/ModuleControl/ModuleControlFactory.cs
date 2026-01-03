// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;

    /// <summary>
    /// Factory for creating MVC module control instances for the MVC pipeline.
    /// </summary>
    internal class ModuleControlFactory
    {
        /// <summary>
        /// Creates an MVC module control instance for the specified module.
        /// </summary>
        /// <param name="module">The module configuration.</param>
        /// <returns>An MVC module control instance.</returns>
        public static IMvcModuleControl CreateModuleControl(ModuleInfo module)
        {
            return CreateModuleControl(module, module.ModuleControl.ControlSrc);
        }

        /// <summary>
        /// Creates an MVC module control instance for the specified module and control source.
        /// </summary>
        /// <param name="module">The module configuration.</param>
        /// <param name="controlSrc">The control source.</param>
        /// <returns>An MVC module control instance.</returns>
        public static IMvcModuleControl CreateModuleControl(ModuleInfo module, string controlSrc)
        {
            IMvcModuleControl control;
            if (!string.IsNullOrEmpty(module.ModuleControl.MvcControlClass))
            {
                var controlClass = module.ModuleControl.MvcControlClass;
                try
                {
                    var obj = Reflection.CreateObject(Globals.GetCurrentServiceProvider(), controlClass, controlClass);
                    if (obj is IMvcModuleControl)
                    {
                        control = obj as IMvcModuleControl;
                    }
                    else
                    {
                        throw new Exception("Mvc Control needs to implement IMvcModuleControl : " + controlClass);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Could not create instance of " + controlClass, ex);
                }
            }
            //// else if (controlSrc.EndsWith(".mvc", System.StringComparison.OrdinalIgnoreCase))
            //// {
            ////     control = new MvcModuleControl();
            //// }
            else if (controlSrc.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                control = new SpaModuleControl();
            }
            else
            {
                throw new Exception("The module control dous not support the MVC pipeline : " + module.ModuleTitle + " " + module.ModuleControl.ControlTitle);
            }

            control.ModuleContext.Configuration = module;
            return control;
        }
    }
}
