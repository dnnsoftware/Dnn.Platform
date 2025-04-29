// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Razor;

using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Compilation;
using System.Web.WebPages;

using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Razor.Helpers;

[DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
public partial class RazorEngine
{
    public RazorEngine(string razorScriptFile, ModuleInstanceContext moduleContext, string localResourceFile)
    {
        this.RazorScriptFile = razorScriptFile;
        this.ModuleContext = moduleContext;
        this.LocalResourceFile = localResourceFile ?? Path.Combine(Path.GetDirectoryName(razorScriptFile), Localization.LocalResourceDirectory, Path.GetFileName(razorScriptFile) + ".resx");

        try
        {
            this.InitWebpage();
        }
        catch (HttpParseException)
        {
            throw;
        }
        catch (HttpCompileException)
        {
            throw;
        }
        catch (Exception exc)
        {
            Exceptions.LogException(exc);
        }
    }

    public DotNetNukeWebPage Webpage { get; set; }

    protected HttpContextBase HttpContext
    {
        get { return new HttpContextWrapper(System.Web.HttpContext.Current); }
    }

    protected string RazorScriptFile { get; set; }

    protected ModuleInstanceContext ModuleContext { get; set; }

    protected string LocalResourceFile { get; set; }

    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    public partial Type RequestedModelType()
    {
        if (this.Webpage != null)
        {
            var webpageType = this.Webpage.GetType();
            if (webpageType.BaseType.IsGenericType)
            {
                return webpageType.BaseType.GetGenericArguments()[0];
            }
        }

        return null;
    }

    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    public partial void Render<T>(TextWriter writer, T model)
    {
        try
        {
            this.Webpage.ExecutePageHierarchy(new WebPageContext(this.HttpContext, this.Webpage, model), writer, this.Webpage);
        }
        catch (Exception exc)
        {
            Exceptions.LogException(exc);
        }
    }

    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    public partial void Render(TextWriter writer)
    {
        try
        {
            this.Webpage.ExecutePageHierarchy(new WebPageContext(this.HttpContext, this.Webpage, null), writer, this.Webpage);
        }
        catch (Exception exc)
        {
            Exceptions.LogException(exc);
        }
    }

    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    public partial void Render(TextWriter writer, WebPageContext context)
    {
        try
        {
            this.Webpage.ExecutePageHierarchy(context, writer, this.Webpage);
        }
        catch (Exception exc)
        {
            Exceptions.LogException(exc);
        }
    }

    private object CreateWebPageInstance()
    {
        var compiledType = BuildManager.GetCompiledType(this.RazorScriptFile);
        object objectValue = null;
        if (compiledType != null)
        {
            objectValue = RuntimeHelpers.GetObjectValue(Activator.CreateInstance(compiledType));
        }

        return objectValue;
    }

    private void InitHelpers(DotNetNukeWebPage webPage)
    {
        webPage.Dnn = new DnnHelper(this.ModuleContext);
        webPage.Html = new HtmlHelper(this.ModuleContext, this.LocalResourceFile);
        webPage.Url = new UrlHelper(this.ModuleContext);
    }

    private void InitWebpage()
    {
        if (!string.IsNullOrEmpty(this.RazorScriptFile))
        {
            var objectValue = RuntimeHelpers.GetObjectValue(this.CreateWebPageInstance());
            if (objectValue == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The webpage found at '{0}' was not created.", new object[] { this.RazorScriptFile }));
            }

            this.Webpage = objectValue as DotNetNukeWebPage;
            if (this.Webpage == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The webpage at '{0}' must derive from DotNetNukeWebPage.", new object[] { this.RazorScriptFile }));
            }

            this.Webpage.Context = this.HttpContext;
            this.Webpage.VirtualPath = VirtualPathUtility.GetDirectory(this.RazorScriptFile);
            this.InitHelpers(this.Webpage);
        }
    }
}
