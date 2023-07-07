// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions
{
    using System;
    
    public class SearchException : BasePortalException
    {
        // default constructor

        /// <summary>Initializes a new instance of the <see cref="SearchException"/> class.</summary>
        public SearchException()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SearchException"/> class.</summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        /// <param name="searchItem"></param>
        public SearchException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
