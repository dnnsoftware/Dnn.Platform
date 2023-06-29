// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens.Models
{
    using DotNetNuke.ComponentModel.DataAnnotations;

    /// <summary>
    /// Class that represents an Api Token Key which tells us for which bits of API this key is valid.
    /// </summary>
    [TableName("ApiTokenKeys")]
    public class ApiTokenKey
    {
        /// <summary>
        /// Gets or sets the ID of the ApiToken.
        /// </summary>
        public int ApiTokenId { get; set; }

        /// <summary>
        /// Gets or sets the token key string. This should be stored in lower case.
        /// </summary>
        public string TokenKey { get; set; }
    }
}
