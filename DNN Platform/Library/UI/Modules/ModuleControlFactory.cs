#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.IO;
using System.Web.UI;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.UI.Modules
{
    public class ModuleControlFactory
    {
        public static Control LoadModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration)
        {
            Control control = null;

            string extension = Path.GetExtension(moduleConfiguration.ModuleControl.ControlSrc.ToLower());

            IModuleControlFactory controlFactory = null;
            switch (extension)
            {
                case ".ascx":
                    controlFactory = new WebFormsModuleControlFactory();
                    break;
                case ".cshtml":
                case ".vbhtml":
                    Type factoryType = Reflection.CreateType("DotNetNuke.Web.Razor.RazorModuleControlFactory");
                    if (factoryType != null)
                    {
                        controlFactory = Reflection.CreateObject(factoryType) as IModuleControlFactory;
                    }
                    break;
                default:
                    // load from a typename in an assembly ( ie. server control)
                    Type objType = Reflection.CreateType(moduleConfiguration.ModuleControl.ControlSrc);
                    control = (containerControl.LoadControl(objType, null));
                    break;
            }

            if (controlFactory != null)
            {
                control = controlFactory.CreateModuleControl(containerControl, moduleConfiguration);
            }

            // set the control ID to the resource file name ( ie. controlname.ascx = controlname )
            // this is necessary for the Localization in PageBase
            if (control != null)
            {
                control.ID = Path.GetFileNameWithoutExtension(moduleConfiguration.ModuleControl.ControlSrc);

                var moduleControl = control as IModuleControl;

                if (moduleControl != null)
                {
                    moduleControl.ModuleContext.Configuration = moduleConfiguration;
                }
            }

            return control;
        }

        public static Control CreateCachedControl(string cachedContent, ModuleInfo moduleConfiguration)
        {
            var moduleControl = new CachedModuleControl(cachedContent);
            moduleControl.ModuleContext.Configuration = moduleConfiguration;
            return moduleControl;
        }

        public static Control CreateModuleControl(ModuleInfo moduleConfiguration)
        {
            var moduleControl = new ModuleControlBase();
            moduleControl.ModuleContext.Configuration = moduleConfiguration;
            return moduleControl;
        }
    }
}