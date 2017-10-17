#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

using System.IO;

namespace DotNetNuke.Web.Client.Providers
{
    using ClientDependency.Core;
    using ClientDependency.Core.CompositeFiles;
    using ClientDependency.Core.CompositeFiles.Providers;

    /// <summary>
    /// A provider for combining, minifying, compressing and saving composite scripts/css files
    /// </summary>
    public class DnnCompositeFileProcessingProvider : CompositeFileProcessingProvider
    {
        private readonly ClientResourceSettings clientResourceSettings = new ClientResourceSettings();

        public override string MinifyFile(string fileContents, ClientDependencyType type)
        {
            switch (type)
            {
                case ClientDependencyType.Css:
                    return MinifyCss ? CssHelper.MinifyCss(fileContents) : fileContents;
                case ClientDependencyType.Javascript:
                {
                    if (!MinifyJs)
                        return fileContents;

                    using (var ms = new MemoryStream())
                    using (var writer = new StreamWriter(ms))
                    {
                        writer.Write(fileContents);
                        writer.Flush();
                        return JSMin.CompressJS(ms);
                    }
                }
                default:
                    return fileContents;
            }
        }

        private bool MinifyCss
        {
            get
            {
                var enableCssMinification = clientResourceSettings.EnableCssMinification();
                return enableCssMinification.HasValue ? enableCssMinification.Value : EnableCssMinify;
            }
        }

        private bool MinifyJs
        {
            get
            {
                var enableJsMinification = clientResourceSettings.EnableJsMinification();
                return enableJsMinification.HasValue ? enableJsMinification.Value : EnableJsMinify;
            }
        }
    }
}