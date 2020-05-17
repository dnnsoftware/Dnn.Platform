﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

using DotNetNuke.Services.Search.Controllers;
using DotNetNuke.Services.Search.Entities;

#endregion

namespace DotNetNuke.Tests.Web.InternalServices
{
    /// <summary>
    /// Search Crawler for Module Content
    /// </summary>
    /// <remarks></remarks>
    public class FakeResultController : BaseResultController
    {
        #region Abstract Class Implmentation

        public override bool HasViewPermission(SearchResult searchResult)
        {
            return true;
        }

        // Returns the URL to the first instance of the module the user has access to view
        public override string GetDocUrl(SearchResult searchResult)
        {
           return "http://www.google.com";
        }

        #endregion
    }

    public class NoPermissionFakeResultController : FakeResultController
    {
        // user has no permission
        public override bool HasViewPermission(SearchResult searchResult)
        {
            // This logic tests that based on ID we give permission to test recursive
            // search in SearchControllerImpl.GetSecurityTrimmedResults() method.
            //The logic here is all documents with AuthorUserId's 6-9, 16-19, 26-29, etc. have
            // permission, while the rest don't
            return (searchResult.AuthorUserId % 10) > 5;
        }
    }
}
