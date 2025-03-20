// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Base class for all CSP directive contributors.
    /// </summary>
    public abstract class BaseCspContributor
    {
        /// <summary>
        /// Gets unique identifier for the contributor.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets type of the CSP directive.
        /// </summary>
        public CspDirectiveType DirectiveType { get; protected set; }

        /// <summary>
        /// Generates the directive string.
        /// </summary>
        /// <returns>The directive string.</returns>
        public abstract string GenerateDirective();
    }
}
