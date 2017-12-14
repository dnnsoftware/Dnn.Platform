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

using System.Net;
using System.Web;

namespace DotNetNuke.Entities.Host
{
    /// <summary>
    /// IServerWebRequestAdapter used to get server's info when new server added into server collections.
    /// Also it can process the request when send to a server, like sync cache, detect server etc.
    /// </summary>
    public interface IServerWebRequestAdapter
    {
        /// <summary>
        /// Get the server's endpoint which can access the server directly.
        /// </summary>
        /// <returns></returns>
        string GetServerUrl();

        /// <summary>
        /// Get the server's unique id when server is behind affinity tool.
        /// </summary>
        /// <returns></returns>
        string GetServerUniqueId();

        /// <summary>
        /// Process Request before the request send to server.
        /// </summary>
        /// <param name="request">The Http Request Object.</param>
        /// <param name="server">The Server Info Object.</param>
        void ProcessRequest(HttpWebRequest request, ServerInfo server);

        /// <summary>
        /// Check whether response is return from correct server.
        /// </summary>
        /// <param name="response">The Http Response Object.</param>
        /// <param name="statusCode">Out status code if you think the status need change.</param>
        void CheckResponse(HttpWebResponse response, ServerInfo server, ref HttpStatusCode statusCode);
    }
}
