
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information


using System;
using System.Collections.Generic;

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

        public GroupedBasicView()
        { }

        public GroupedBasicView(BasicView basic)
        {
            this.DocumentTypeName = basic.DocumentTypeName;
            this.Results = new List<BasicView>
            {
                new BasicView
                {
                    Title = basic.Title,
                    Snippet = basic.Snippet,
                    Description = basic.Description,
                    DocumentUrl = basic.DocumentUrl,
                    Attributes = basic.Attributes
                },
            };
        }
    }
}
