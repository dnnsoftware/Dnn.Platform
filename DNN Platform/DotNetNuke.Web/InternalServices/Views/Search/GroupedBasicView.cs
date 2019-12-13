// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Web.InternalServices.Views.Search
{
    /// <summary>
    /// BasicView grouped by DocumentTypeName
    /// </summary>
    public class GroupedBasicView
    {
        /// <summary>
        /// Type of Search Document
        /// </summary>
        public string DocumentTypeName { get; set; }

        /// <summary>
        /// Results of the Search
        /// </summary>   
        public List<BasicView> Results { get; set; }

        #region constructor

        public GroupedBasicView()
        {}

        public GroupedBasicView(BasicView basic)
        {
            DocumentTypeName = basic.DocumentTypeName;
            Results = new List<BasicView>
            {
                new BasicView
                {
                    Title = basic.Title,
                    Snippet = basic.Snippet,
                    Description = basic.Description,
                    DocumentUrl = basic.DocumentUrl,
                    Attributes = basic.Attributes
                }
            };
        }

        #endregion
    }
}
