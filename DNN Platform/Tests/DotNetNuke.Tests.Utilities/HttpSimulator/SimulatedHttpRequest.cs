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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace DotNetNuke.Tests.Instance.Utilities.HttpSimulator
{
	/// <summary>
	/// Used to simulate an HttpRequest.
	/// </summary>
	public class SimulatedHttpRequest : SimpleWorkerRequest
	{
	    Uri _referer;
	    readonly string _host;
	    readonly string _verb;
	    readonly int _port;
	    readonly string _physicalFilePath;

        /// <summary>
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
	    public SimulatedHttpRequest(string applicationPath, string physicalAppPath, string physicalFilePath, string page, string query, TextWriter output, string host, int port, string verb) : base(applicationPath, physicalAppPath, page, query, output)
	    {
            Form = new NameValueCollection();
            Headers = new NameValueCollection();
            if (host == null)
                throw new ArgumentNullException("host", "Host cannot be null.");

            if(host.Length == 0)
                throw new ArgumentException("Host cannot be empty.", "host");

            if (applicationPath == null)
                throw new ArgumentNullException("applicationPath", "Can't create a request with a null application path. Try empty string.");

            _host = host;
            _verb = verb;
	        _port = port;
            _physicalFilePath = physicalFilePath;
	    }

        internal void SetReferer(Uri referer)
        {
            _referer = referer;
        }

	    /// <summary>
		/// Returns the specified member of the request header.
		/// </summary>
		/// <returns>
		/// The HTTP verb returned in the request
		/// header.
		/// </returns>
		public override string GetHttpVerbName()
		{
			return _verb;
		}
		
		/// <summary>
		/// Gets the name of the server.
		/// </summary>
		/// <returns></returns>
		public override string GetServerName()
		{
			return _host;
		}
	    
	    public override int GetLocalPort()
	    {
            return _port;
	    }

        public override bool IsSecure()
        {
            return (_port == 443) ? true : false;
        }

        //public override string GetProtocol()
        //{
        //    return (_port == 443) ? "https:" : "http:";
        //}

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
		/// Get all nonstandard HTTP header name-value pairs.
		/// </summary>
		/// <returns>An array of header name-value pairs.</returns>
		public override string[][] GetUnknownRequestHeaders()
		{
			if(Headers == null || Headers.Count == 0)
			{
				return null;
			}
			var headersArray = new string[Headers.Count][];
			for(var i = 0; i < Headers.Count; i++)
			{
				headersArray[i] = new string[2];
				headersArray[i][0] = Headers.Keys[i];
				headersArray[i][1] = Headers[i];
			}
			return headersArray;
		}

        public override string GetKnownRequestHeader(int index)
        {
            if (index == 0x24)
				return _referer == null ? string.Empty : _referer.ToString();

            if (index == 12 && _verb == "POST")
                return "application/x-www-form-urlencoded";
            
            return base.GetKnownRequestHeader(index);
        }

	    public override string GetFilePathTranslated()
        {
            return _physicalFilePath;
        }
		
		/// <summary>
		/// Reads request data from the client (when not preloaded).
		/// </summary>
		/// <returns>The number of bytes read.</returns>
		public override byte[] GetPreloadedEntityBody()
		{
			return Encoding.UTF8.GetBytes(Form.Keys.Cast<string>().Aggregate(string.Empty, (current, key) => current + string.Format("{0}={1}&", key, Form[key])));
		}
		
		/// <summary>
		/// Returns a value indicating whether all request data
		/// is available and no further reads from the client are required.
		/// </summary>
		/// <returns>
		/// 	<see langword="true"/> if all request data is available; otherwise,
		/// <see langword="false"/>.
		/// </returns>
		public override bool IsEntireEntityBodyIsPreloaded()
		{
			return true;
		}
	}
}
