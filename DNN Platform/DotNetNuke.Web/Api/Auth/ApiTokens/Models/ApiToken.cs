// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens.Models
{
    using DotNetNuke.ComponentModel.DataAnnotations;

    /// <summary>
    /// Class that represents an Api Token.
    /// </summary>
    /// <remarks>
    /// This class is used to retrieve data regarding an API token.
    /// </remarks>
    [TableName("vw_ApiTokens")]
    [PrimaryKey("ApiTokenId", AutoIncrement = true)]
    [Scope("PortalId")]
    public class ApiToken : ApiTokenBase
    {
        /// <summary>
        /// Gets or sets the name of the portal of this API token if it is not a host level token.
        /// </summary>
        public string PortalName { get; set; }

        /// <summary>
        /// Gets or sets the user displayname who created the API token.
        /// </summary>
        public string CreatedByUser { get; set; }

        /// <summary>
        /// Gets or sets the username of the user who created the API token.
        /// </summary>
        public string CreatedByUsername { get; set; }

        /// <summary>
        /// Gets or sets the user displayname who revoked the API token.
        /// </summary>
        public string RevokedByUser { get; set; }

        /// <summary>
        /// Gets or sets the username of the user who revoked the API token.
        /// </summary>
        public string RevokedByUsername { get; set; }

        /// <summary>
        /// Gets or sets a comma-separated list of API keys for this token.
        /// </summary>
        public string Keys { get; set; }
    }
}
