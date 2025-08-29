namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    internal class StylesheetInclude : FileInclude
    {
        /// <summary>
        /// Indicates whether the described stylesheet should be loaded and applied to the document. 
        /// If disabled is specified in the HTML when it is loaded, the stylesheet will not be loaded during page load. 
        /// Instead, the stylesheet will be loaded on-demand, if and when the disabled attribute is changed to false or removed.
        /// </summary>
        public bool Disabled { get; set; } = false;
    }
}
