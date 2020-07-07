// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Compression
{
    using System.IO;
    using System.Web;

    /// <summary>
    /// Base for any HttpFilter that performing compression.
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
        /// Initializes a new instance of the <see cref="CompressingFilter"/> class.
        /// Protected constructor that sets up the underlying stream we're compressing into.
        /// </summary>
        /// <param name="baseStream">The stream we're wrapping up.</param>
        protected CompressingFilter(Stream baseStream)
            : base(baseStream)
        {
        }

        /// <summary>
        /// Gets the name of the content-encoding that's being implemented.
        /// </summary>
        /// <remarks>
        /// See http://www.w3.org/Protocols/rfc2616/rfc2616-sec3.html#sec3.5 for more
        /// details on content codings.
        /// </remarks>
        public abstract string ContentEncoding { get; }

        /// <summary>
        /// Gets a value indicating whether keeps track of whether or not we're written the compression headers.
        /// </summary>
        protected bool HasWrittenHeaders
        {
            get
            {
                return this._HasWrittenHeaders;
            }
        }

        /// <summary>
        /// Writes out the compression-related headers.  Subclasses should call this once before writing to the output stream.
        /// </summary>
        internal void WriteHeaders()
        {
            // this is dangerous.  if Response.End is called before the filter is used, directly or indirectly,
            // the content will not pass through the filter.  However, this header will still be appended.
            // Look for handling cases in PreRequestSendHeaders and Pre
            HttpContext.Current.Response.AppendHeader("Content-Encoding", this.ContentEncoding);
            HttpContext.Current.Response.AppendHeader("X-Compressed-By", "DotNetNuke-Compression");
            this._HasWrittenHeaders = true;
        }
    }
}
