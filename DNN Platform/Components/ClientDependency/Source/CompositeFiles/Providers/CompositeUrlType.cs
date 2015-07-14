namespace ClientDependency.Core.CompositeFiles.Providers
{
    public enum CompositeUrlType
    {
        /// <summary>
        /// The original URL type in which full dependency paths are base64 encoded as query strings
        /// </summary>
        Base64QueryStrings,

        /// <summary>
        /// Creates a URL in which the full dependency paths are base64 encoded as URL paths, however because
        /// paths can get quite large, this requires that .Net 4 is running and that you increase the maxUrlLength
        /// configuration property in the httpRuntime section in your web.config
        /// </summary>
        Base64Paths,
        
        /// <summary>
        /// Uses the file map provider to store and map the dependency paths with a reference to an ID it generates
        /// </summary>
        MappedId
    }
}