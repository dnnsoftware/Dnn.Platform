#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;

using DotNetNuke.Services.Search.Controllers;
using DotNetNuke.Services.Search.Entities;

#endregion

namespace DotNetNuke.Tests.Core.Controllers.Search
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
