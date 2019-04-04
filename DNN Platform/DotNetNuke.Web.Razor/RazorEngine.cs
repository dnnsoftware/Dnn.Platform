﻿#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
#region Using

using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Compilation;
using System.Web.WebPages;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Razor.Helpers;

#endregion

namespace DotNetNuke.Web.Razor
{
    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public class RazorEngine
    {
        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public RazorEngine(string razorScriptFile, ModuleInstanceContext moduleContext, string localResourceFile)
        {
            RazorScriptFile = razorScriptFile;
            ModuleContext = moduleContext;
            LocalResourceFile = localResourceFile ?? Path.Combine(Path.GetDirectoryName(razorScriptFile), Localization.LocalResourceDirectory, Path.GetFileName(razorScriptFile) + ".resx");

            try
            {
                InitWebpage();
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

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected string RazorScriptFile { get; set; }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected ModuleInstanceContext ModuleContext { get; set; }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected string LocalResourceFile { get; set; }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public DotNetNukeWebPage Webpage { get; set; }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected HttpContextBase HttpContext
        {
            get { return new HttpContextWrapper(System.Web.HttpContext.Current); }
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public Type RequestedModelType()
        {
            if (Webpage != null)
            {
                var webpageType = Webpage.GetType();
                if (webpageType.BaseType.IsGenericType)
                {
                    return webpageType.BaseType.GetGenericArguments()[0];
                }
            }
            return null;
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public void Render<T>(TextWriter writer, T model)
        {
            try
            {
                Webpage.ExecutePageHierarchy(new WebPageContext(HttpContext, Webpage, model), writer, Webpage);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public void Render(TextWriter writer)
        {
            try
            {
                Webpage.ExecutePageHierarchy(new WebPageContext(HttpContext, Webpage, null), writer, Webpage);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public void Render(TextWriter writer, WebPageContext context)
        {
            try
            {
                Webpage.ExecutePageHierarchy(context, writer, Webpage);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }

        private object CreateWebPageInstance()
        {
            var compiledType = BuildManager.GetCompiledType(RazorScriptFile);
            object objectValue = null;
            if (((compiledType != null)))
            {
                objectValue = RuntimeHelpers.GetObjectValue(Activator.CreateInstance(compiledType));
            }
            return objectValue;
        }

        private void InitHelpers(DotNetNukeWebPage webPage)
        {
            webPage.Dnn = new DnnHelper(ModuleContext);
            webPage.Html = new HtmlHelper(ModuleContext, LocalResourceFile);
            webPage.Url = new UrlHelper(ModuleContext);
        }

        private void InitWebpage()
        {
            if (!string.IsNullOrEmpty(RazorScriptFile))
            {
                var objectValue = RuntimeHelpers.GetObjectValue(CreateWebPageInstance());
                if ((objectValue == null))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The webpage found at '{0}' was not created.", new object[] {RazorScriptFile}));
                }
                Webpage = objectValue as DotNetNukeWebPage;
                if ((Webpage == null))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The webpage at '{0}' must derive from DotNetNukeWebPage.", new object[] {RazorScriptFile}));
                }
                Webpage.Context = HttpContext;
                Webpage.VirtualPath = VirtualPathUtility.GetDirectory(RazorScriptFile);
                InitHelpers(Webpage);
            }
        }
    }
}