#region Usings

using System;
using System.Collections.Generic;
using DotNetNuke.Services.Search.Internals;

#endregion

namespace DotNetNuke.Web.InternalServices.Views.Search
{
    /// <summary>
    /// Detailed Search Result View
    /// </summary>
    public class BasicView
    {
        /// <summary>
        /// Document's Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Highlighted snippet from document
        /// </summary>
        public string Snippet { get; set; }

        /// <summary>
        /// Description from document
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Link to the Document
        /// </summary>
        public string DocumentUrl { get; set; }

        /// <summary>
        /// Display Name of the Document Type
        /// </summary>
        public string DocumentTypeName { get; set; }

        /// <summary>
        /// Custom Attributes of the document.
        /// </summary>
        public IDictionary<string, string> Attributes { get; set; }

        public BasicView()
        {
            Attributes = new Dictionary<string, string>();
        }

    }
}
