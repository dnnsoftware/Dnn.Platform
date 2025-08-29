namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    internal class ScriptInclude : FileInclude
    {
        /// <summary>
        /// For classic scripts, if the async attribute is present, then the classic script will be fetched in parallel to parsing and evaluated as soon as it is available.
        /// For module scripts, if the async attribute is present then the scripts and all their dependencies will be fetched in parallel to parsing and evaluated as soon as they are available.
        /// If the attribute is specified with the defer attribute, the element will act as if only the async attribute is specified.
        /// </summary>
        public bool Async { get; set; }

        /// <summary>
        /// This Boolean attribute is set to indicate to a browser that the script is meant to be executed after the document has been parsed, but before firing DOMContentLoaded event.
        /// Scripts with the defer attribute will prevent the DOMContentLoaded event from firing until the script has loaded and finished evaluating.
        /// Scripts with the defer attribute will execute in the order in which they appear in the document.
        /// If the attribute is specified with the async attribute, the element will act as if only the async attribute is specified.
        /// </summary>
        public bool Defer { get; set; }

        /// <summary>
        /// This Boolean attribute is set to indicate that the script should not be executed in browsers that support ES modules — in effect, 
        /// this can be used to serve fallback scripts to older browsers that do not support modular JavaScript code.
        /// </summary>
        public bool NoModule { get; set; }
    }
}
