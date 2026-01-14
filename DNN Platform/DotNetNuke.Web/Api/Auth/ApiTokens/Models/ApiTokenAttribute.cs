// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens.Models
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>This class is used to retrieve information about available APIs for Api Tokens.</summary>
    [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
    public class ApiTokenAttribute
    {
        /// <summary>Initializes a new instance of the <see cref="ApiTokenAttribute"/> class.</summary>
        /// <param name="scope">The scope of the API token attribute.</param>
        /// <param name="key">The API key string.</param>
        /// <param name="name">The localized name of this part of the API.</param>
        /// <param name="description">The localized description of this part of the API.</param>
        public ApiTokenAttribute(int scope, string key, string name, string description)
        {
            this.Scope = scope;
            this.Key = key;
            this.Name = name;
            this.Description = description;
        }

        /// <summary>Gets or sets the scope of the API token attribute.</summary>
        /// <remarks>The scope is set to determine the API requests allowed for this part of the API.</remarks>
        public int Scope { get; set; }

        /// <summary>Gets or sets the API key string.</summary>
        public string Key { get; set; }

        /// <summary>Gets or sets the localized name of this part of the API.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the localized description of this part of the API.</summary>
        public string Description { get; set; }
    }
}
