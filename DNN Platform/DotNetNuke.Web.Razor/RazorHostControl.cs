// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Razor;

using System;
using System.IO;
using System.Web;
using System.Web.UI;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.UI.Modules;

[DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
public partial class RazorHostControl : ModuleControlBase, IActionable
{
    private readonly string razorScriptFile;

    private RazorEngine engine;

    /// <summary>Initializes a new instance of the <see cref="RazorHostControl"/> class.</summary>
    /// <param name="scriptFile">The path to the Razor script file.</param>
    public RazorHostControl(string scriptFile)
    {
        this.razorScriptFile = scriptFile;
    }

    /// <inheritdoc/>
    public ModuleActionCollection ModuleActions
    {
        get
        {
            if (this.Engine.Webpage is IActionable)
            {
                return (this.Engine.Webpage as IActionable).ModuleActions;
            }

            return new ModuleActionCollection();
        }
    }

    protected virtual string RazorScriptFile
    {
        get { return this.razorScriptFile; }
    }

    private RazorEngine Engine
    {
        get
        {
            if (this.engine == null)
            {
                this.engine = new RazorEngine(this.RazorScriptFile, this.ModuleContext, this.LocalResourceFile);
            }

            return this.engine;
        }
    }

    /// <inheritdoc/>
    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    protected override partial void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        if (!string.IsNullOrEmpty(this.RazorScriptFile))
        {
            var writer = new StringWriter();
            this.Engine.Render(writer);
            this.Controls.Add(new LiteralControl(HttpUtility.HtmlDecode(writer.ToString())));
        }
    }
}
