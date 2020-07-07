// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.Providers
{
    using System;
    using System.IO;

    using ClientDependency.Core;
    using ClientDependency.Core.CompositeFiles;
    using ClientDependency.Core.CompositeFiles.Providers;

    /// <summary>
    /// A provider for combining, minifying, compressing and saving composite scripts/css files.
    /// </summary>
    public class DnnCompositeFileProcessingProvider : CompositeFileProcessingProvider
    {
        private readonly ClientResourceSettings clientResourceSettings = new ClientResourceSettings();

        private bool MinifyCss
        {
            get
            {
                var enableCssMinification = this.clientResourceSettings.EnableCssMinification();
                return enableCssMinification.HasValue ? enableCssMinification.Value : this.EnableCssMinify;
            }
        }

        private bool MinifyJs
        {
            get
            {
                var enableJsMinification = this.clientResourceSettings.EnableJsMinification();
                return enableJsMinification.HasValue ? enableJsMinification.Value : this.EnableJsMinify;
            }
        }

        public override string MinifyFile(Stream fileStream, ClientDependencyType type)
        {
            Func<Stream, string> streamToString = stream =>
            {
                if (!stream.CanRead)
                {
                    throw new InvalidOperationException("Cannot read input stream");
                }

                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }

                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            };

            switch (type)
            {
                case ClientDependencyType.Css:
                    return this.MinifyCss ? CssHelper.MinifyCss(fileStream) : streamToString(fileStream);
                case ClientDependencyType.Javascript:
                    return this.MinifyJs ? JSMin.CompressJS(fileStream) : streamToString(fileStream);
                default:
                    return streamToString(fileStream);
            }
        }

        public override string MinifyFile(string fileContents, ClientDependencyType type)
        {
            switch (type)
            {
                case ClientDependencyType.Css:
                    return this.MinifyCss ? CssHelper.MinifyCss(fileContents) : fileContents;
                case ClientDependencyType.Javascript:
                    {
                        if (!this.MinifyJs)
                        {
                            return fileContents;
                        }

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
    }
}
