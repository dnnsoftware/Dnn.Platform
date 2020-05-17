﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;
using DotNetNuke.Services.Search.Entities;

#endregion

namespace DotNetNuke.Services.Search.Controllers
{
    /// <summary>
    /// BaseResult to be implemented by the different Crawlers to provide Permission and Url Services 
    /// </summary>
    /// <remarks>The abstract methods in this Class will be called by Search Result engine for every Hit found in Search Index.</remarks>
    [Serializable]
    public abstract class BaseResultController
    {
        #region Abstract

        /// <summary>
        /// Does the user in the Context have View Permission on the Document
        /// </summary>
        /// <param name="searchResult">Search Result</param>
        /// <returns>True or False</returns>
        public abstract bool HasViewPermission(SearchResult searchResult);

        /// <summary>
        /// Return a Url that can be shown in search results.
        /// </summary>
        /// <param name="searchResult">Search Result</param>
        /// <returns>Url</returns>
        /// <remarks>The Query Strings in the Document (if present) should be appended while returning the Url</remarks>
        public abstract string GetDocUrl(SearchResult searchResult);
        
        /// <summary>
        /// Gets the localized search type name.
        /// </summary>
        public virtual string LocalizedSearchTypeName => "";

        #endregion
    }
}
