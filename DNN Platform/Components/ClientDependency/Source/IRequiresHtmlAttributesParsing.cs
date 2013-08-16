namespace ClientDependency.Core
{
    /// <summary>
    /// interface defining that an IClientDependencyFile has html attributes applied as a string which require parsing
    /// </summary>
    public interface IRequiresHtmlAttributesParsing : IHaveHtmlAttributes
    {
        /// <summary>
        /// Used to set the HtmlAttributes on this class via a string which is parsed
        /// </summary>
        /// <remarks>
        /// The syntax for the string must be: key1:value1,key2:value2   etc...
        /// </remarks>
        string HtmlAttributesAsString { get; set; }
    }
}