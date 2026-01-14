// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ComponentModel.DataAnnotations
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ScopeAttribute : Attribute
    {
        /// <summary>Initializes a new instance of the <see cref="ScopeAttribute"/> class.</summary>
        /// <param name="scope">The name of the column/property which defines the scope for the cache.</param>
        public ScopeAttribute(string scope)
        {
            this.Scope = scope;
        }

        /// <summary>Gets or sets the property to use to scope the cache.  The default is an empty string.</summary>
        public string Scope { get; set; }
    }
}
