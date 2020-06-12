// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;

namespace DotNetNuke.ComponentModel.DataAnnotations
{
    public class ScopeAttribute : Attribute
    {
        public ScopeAttribute(string scope)
        {
            this.Scope = scope;
        }

        /// <summary>
        /// The property to use to scope the cache.  The default is an empty string.
        /// </summary>
        public string Scope { get; set; }

    }
}
