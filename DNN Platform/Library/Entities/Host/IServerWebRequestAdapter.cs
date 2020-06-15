// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Host
{
    using System.Net;
    using System.Web;

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
