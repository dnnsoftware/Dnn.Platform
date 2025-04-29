// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Razor;

using System;
using System.IO;
using System.Web.UI;

using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Modules;

[DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
public partial class RazorModuleBase : ModuleUserControlBase
{
    protected virtual string RazorScriptFile
    {
        get
        {
            var scriptFolder = this.AppRelativeTemplateSourceDirectory;
            var fileRoot = Path.GetFileNameWithoutExtension(this.AppRelativeVirtualPath);
            var scriptFile = scriptFolder + "_" + fileRoot + ".cshtml";

            if (!File.Exists(this.Server.MapPath(scriptFile)))
            {
                // Try VB (vbhtml)
                scriptFile = scriptFolder + "_" + fileRoot + ".vbhtml";

                if (!File.Exists(this.Server.MapPath(scriptFile)))
                {
                    // Return ""
                    scriptFile = string.Empty;
                }
            }

            return scriptFile;
        }
    }

    /// <inheritdoc/>
    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    protected override partial void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);
        try
        {
            if (!string.IsNullOrEmpty(this.RazorScriptFile))
            {
                var razorEngine = new RazorEngine(this.RazorScriptFile, this.ModuleContext, this.LocalResourceFile);
                var writer = new StringWriter();
                razorEngine.Render(writer);

                this.Controls.Add(new LiteralControl(writer.ToString()));
            }
        }
        catch (Exception ex)
        {
            Exceptions.ProcessModuleLoadException(this, ex);
        }
    }
}
