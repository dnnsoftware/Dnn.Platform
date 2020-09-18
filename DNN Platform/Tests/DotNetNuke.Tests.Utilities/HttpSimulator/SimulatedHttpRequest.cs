// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Instance.Utilities.HttpSimulator
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web.Hosting;

    /// <summary>
    /// Used to simulate an HttpRequest.
    /// </summary>
    public class SimulatedHttpRequest : SimpleWorkerRequest
    {
        private readonly string _host;
        private readonly string _verb;
        private readonly int _port;
        private readonly string _physicalFilePath;
        private Uri _referer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimulatedHttpRequest"/> class.
        /// Creates a new <see cref="SimulatedHttpRequest"/> instance.
        /// </summary>
        /// <param name="applicationPath">App virtual dir.</param>
        /// <param name="physicalAppPath">Physical Path to the app.</param>
        /// <param name="physicalFilePath">Physical Path to the file.</param>
        /// <param name="page">The Part of the URL after the application.</param>
        /// <param name="query">Query.</param>
        /// <param name="output">Output.</param>
        /// <param name="host">Host.</param>
        /// <param name="port">Port to request.</param>
        /// <param name="verb">The HTTP Verb to use.</param>
        public SimulatedHttpRequest(string applicationPath, string physicalAppPath, string physicalFilePath, string page, string query, TextWriter output, string host, int port, string verb)
            : base(applicationPath, physicalAppPath, page, query, output)
        {
            this.Form = new NameValueCollection();
            this.Headers = new NameValueCollection();
            if (host == null)
            {
                throw new ArgumentNullException("host", "Host cannot be null.");
            }

            if (host.Length == 0)
            {
                throw new ArgumentException("Host cannot be empty.", "host");
            }

            if (applicationPath == null)
            {
                throw new ArgumentNullException("applicationPath", "Can't create a request with a null application path. Try empty string.");
            }

            this._host = host;
            this._verb = verb;
            this._port = port;
            this._physicalFilePath = physicalFilePath;
        }

        // public override string GetProtocol()
        // {
        //    return (_port == 443) ? "https:" : "http:";
        // }

        /// <summary>
        /// Gets the headers.
        /// </summary>
        /// <value>The headers.</value>
        public NameValueCollection Headers { get; private set; }

        /// <summary>
        /// Gets the format exception.
        /// </summary>
        /// <value>The format exception.</value>
        public NameValueCollection Form { get; private set; }

        /// <summary>
        /// Returns the specified member of the request header.
        /// </summary>
        /// <returns>
        /// The HTTP verb returned in the request
        /// header.
        /// </returns>
        public override string GetHttpVerbName()
        {
            return this._verb;
        }

        /// <summary>
        /// Gets the name of the server.
        /// </summary>
        /// <returns></returns>
        public override string GetServerName()
        {
            return this._host;
        }

        public override int GetLocalPort()
        {
            return this._port;
        }

        public override bool IsSecure()
        {
            return (this._port == 443) ? true : false;
        }

        /// <summary>
        /// Get all nonstandard HTTP header name-value pairs.
        /// </summary>
        /// <returns>An array of header name-value pairs.</returns>
        public override string[][] GetUnknownRequestHeaders()
        {
            if (this.Headers == null || this.Headers.Count == 0)
            {
                return null;
            }

            var headersArray = new string[this.Headers.Count][];
            for (var i = 0; i < this.Headers.Count; i++)
            {
                headersArray[i] = new string[2];
                headersArray[i][0] = this.Headers.Keys[i];
                headersArray[i][1] = this.Headers[i];
            }

            return headersArray;
        }

        public override string GetKnownRequestHeader(int index)
        {
            if (index == 0x24)
            {
                return this._referer == null ? string.Empty : this._referer.ToString();
            }

            if (index == 12 && this._verb == "POST")
            {
                return "application/x-www-form-urlencoded";
            }

            return base.GetKnownRequestHeader(index);
        }

        public override string GetFilePathTranslated()
        {
            return this._physicalFilePath;
        }

        /// <summary>
        /// Reads request data from the client (when not preloaded).
        /// </summary>
        /// <returns>The number of bytes read.</returns>
        public override byte[] GetPreloadedEntityBody()
        {
            return Encoding.UTF8.GetBytes(this.Form.Keys.Cast<string>().Aggregate(string.Empty, (current, key) => current + string.Format("{0}={1}&", key, this.Form[key])));
        }

        /// <summary>
        /// Returns a value indicating whether all request data
        /// is available and no further reads from the client are required.
        /// </summary>
        /// <returns>
        ///     <see langword="true"/> if all request data is available; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool IsEntireEntityBodyIsPreloaded()
        {
            return true;
        }

        internal void SetReferer(Uri referer)
        {
            this._referer = referer;
        }
    }
}
