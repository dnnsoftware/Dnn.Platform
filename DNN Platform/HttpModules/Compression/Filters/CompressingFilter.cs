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
#region Usings

using System.IO;
using System.Web;

#endregion

namespace DotNetNuke.HttpModules.Compression
{
    /// <summary>
    /// Base for any HttpFilter that performing compression
    /// </summary>
    /// <remarks>
    /// When implementing this class, you need to implement a <see cref="HttpOutputFilter"/>
    /// along with a <see cref="CompressingFilter.ContentEncoding"/>.  The latter corresponds to a 
    /// content coding (see http://www.w3.org/Protocols/rfc2616/rfc2616-sec3.html#sec3.5)
    /// that your implementation will support.
    /// </remarks>
    public abstract class CompressingFilter : HttpOutputFilter
    {
        private bool _HasWrittenHeaders;

        /// <summary>
        /// Protected constructor that sets up the underlying stream we're compressing into
        /// </summary>
        /// <param name="baseStream">The stream we're wrapping up</param>
        protected CompressingFilter(Stream baseStream) : base(baseStream)
        {
        }

        /// <summary>
        /// The name of the content-encoding that's being implemented
        /// </summary>
        /// <remarks>
        /// See http://www.w3.org/Protocols/rfc2616/rfc2616-sec3.html#sec3.5 for more
        /// details on content codings.
        /// </remarks>
        public abstract string ContentEncoding { get; }

        /// <summary>
        /// Keeps track of whether or not we're written the compression headers
        /// </summary>
        protected bool HasWrittenHeaders
        {
            get
            {
                return _HasWrittenHeaders;
            }
        }

        /// <summary>
        /// Writes out the compression-related headers.  Subclasses should call this once before writing to the output stream.
        /// </summary>
        internal void WriteHeaders()
        {
            //this is dangerous.  if Response.End is called before the filter is used, directly or indirectly,
            //the content will not pass through the filter.  However, this header will still be appended.  
            //Look for handling cases in PreRequestSendHeaders and Pre
            HttpContext.Current.Response.AppendHeader("Content-Encoding", ContentEncoding);
            HttpContext.Current.Response.AppendHeader("X-Compressed-By", "DotNetNuke-Compression");
            _HasWrittenHeaders = true;
        }
    }
}
