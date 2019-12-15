// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
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

        public override string MinifyFile(Stream fileStream, ClientDependencyType type)
        {
            Func<Stream, string> streamToString = stream =>
            {
                if (!stream.CanRead)
                    throw new InvalidOperationException("Cannot read input stream");

                if (stream.CanSeek)
                    stream.Position = 0;

                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            };

            switch (type)
            {
                case ClientDependencyType.Css:
                    return MinifyCss ? CssHelper.MinifyCss(fileStream) : streamToString(fileStream);
                case ClientDependencyType.Javascript:
                    return MinifyJs ? JSMin.CompressJS(fileStream) : streamToString(fileStream);
                default:
                    return streamToString(fileStream);
            }
        }

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
