// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

using DotNetNuke.Services.Search;

#endregion

namespace DotNetNuke.Services.Exceptions
{
    #pragma warning disable 0618
    public class SearchException : BasePortalException
    {
        //default constructor
		public SearchException()
        {
        }

        public SearchException(string message, Exception inner) : base(message, inner)
        {
           
        }
    }
    #pragma warning restore 0618
}
