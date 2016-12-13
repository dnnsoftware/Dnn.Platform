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
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DotNetNuke.Web.Api
{
    /// <summary>
    /// A MediaTypeFormatter that simply allows strings to pass through WebAPI and be associated with the specified MIME type
    /// </summary>
    public class StringPassThroughMediaTypeFormatter : MediaTypeFormatter
    {
        /// <summary>
        /// Initialize a formatter that can handle text/plain and text/html
        /// </summary>
        public StringPassThroughMediaTypeFormatter() : this(new [] {"text/plain", "text/html"}) {}

        /// <summary>
        /// Initialize a formatter that can handle the specified media types
        /// </summary>
        public StringPassThroughMediaTypeFormatter(IEnumerable<string> mediaTypes)
        {
            foreach (var type in mediaTypes)
            {
                SupportedMediaTypes.Add(new MediaTypeHeaderValue(type));
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
