// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    /// <summary>
    /// A MediaTypeFormatter that simply allows strings to pass through WebAPI and be associated with the specified MIME type.
    /// </summary>
    public class StringPassThroughMediaTypeFormatter : MediaTypeFormatter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringPassThroughMediaTypeFormatter"/> class.
        /// Initialize a formatter that can handle text/plain and text/html.
        /// </summary>
        public StringPassThroughMediaTypeFormatter()
            : this(new[] { "text/plain", "text/html" })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringPassThroughMediaTypeFormatter"/> class.
        /// Initialize a formatter that can handle the specified media types.
        /// </summary>
        public StringPassThroughMediaTypeFormatter(IEnumerable<string> mediaTypes)
        {
            foreach (var type in mediaTypes)
            {
                this.SupportedMediaTypes.Add(new MediaTypeHeaderValue(type));
            }
        }

        public override bool CanReadType(Type type)
        {
            return type == typeof(string);
        }

        public override bool CanWriteType(Type type)
        {
            return type == typeof(string);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger)
        {
            using (var reader = new StreamReader(readStream))
            {
                string value = reader.ReadToEnd();

                var completionSource = new TaskCompletionSource<object>();
                completionSource.SetResult(value);
                return completionSource.Task;
            }
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, TransportContext transportContext)
        {
            using (var writer = new StreamWriter(writeStream))
            {
                writer.Write((string)value);
                writer.Flush();
            }

            var completionSource = new TaskCompletionSource<object>();
            completionSource.SetResult(null);
            return completionSource.Task;
        }
    }
}
