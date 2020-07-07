// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.InternalServices.Views.Search
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// BasicView grouped by DocumentTypeName.
    /// </summary>
    public class GroupedBasicView
    {
        public GroupedBasicView()
        {
        }

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

        /// <summary>
        /// Gets or sets type of Search Document.
        /// </summary>
        public string DocumentTypeName { get; set; }

        /// <summary>
        /// Gets or sets results of the Search.
        /// </summary>
        public List<BasicView> Results { get; set; }
    }
}
