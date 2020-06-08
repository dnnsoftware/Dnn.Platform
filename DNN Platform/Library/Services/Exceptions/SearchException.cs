// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

using DotNetNuke.Services.Search;

#endregion

namespace DotNetNuke.Services.Exceptions
{
    #pragma warning disable 0618
    public class SearchException : BasePortalException
    {
        private readonly SearchItemInfo m_SearchItem;

        //default constructor
		public SearchException()
        {
        }

        public SearchException(string message, Exception inner, SearchItemInfo searchItem) : base(message, inner)
        {
            this.m_SearchItem = searchItem;
        }

        public SearchItemInfo SearchItem
        {
            get
            {
                return this.m_SearchItem;
            }
        }
    }
    #pragma warning restore 0618
}
