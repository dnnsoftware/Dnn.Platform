using System.Collections.Generic;

namespace ClientDependency.Core
{
    /// <summary>
    /// interface defining that an object has Html attributes
    /// </summary>
    public interface IHaveHtmlAttributes
    {

        /// <summary>
        /// Used to store additional attributes in the HTML markup for the item
        /// </summary>
        /// <remarks>
        /// Mostly used for CSS Media, but could be for anything
        /// </remarks>
        IDictionary<string, string> HtmlAttributes { get; }

    }
}