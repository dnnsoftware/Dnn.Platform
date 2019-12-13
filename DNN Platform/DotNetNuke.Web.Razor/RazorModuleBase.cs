﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.IO;
using System.Web.UI;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.Web.Razor
{
    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public class RazorModuleBase : ModuleUserControlBase
    {
        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected virtual string RazorScriptFile
        {
            get
            {
                var scriptFolder = AppRelativeTemplateSourceDirectory;
                var fileRoot = Path.GetFileNameWithoutExtension(AppRelativeVirtualPath);
                var scriptFile = scriptFolder + "_" + fileRoot + ".cshtml";

                if (! (File.Exists(Server.MapPath(scriptFile))))
                {
                    //Try VB (vbhtml)
                    scriptFile = scriptFolder + "_" + fileRoot + ".vbhtml";

                    if (!(File.Exists(Server.MapPath(scriptFile))))
                    {
                        //Return ""
                        scriptFile = "";
                    }
                }

                return scriptFile;
            }
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            try
            {
                if (! (string.IsNullOrEmpty(RazorScriptFile)))
                {
                    var razorEngine = new RazorEngine(RazorScriptFile, ModuleContext, LocalResourceFile);
                    var writer = new StringWriter();
                    razorEngine.Render(writer);

                    Controls.Add(new LiteralControl(writer.ToString()));
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }
    }
}
