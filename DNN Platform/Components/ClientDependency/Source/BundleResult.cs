using System.Collections.Generic;

namespace ClientDependency.Core
{
    /// <summary>
    /// Is returned when resolving a bundle
    /// </summary>
    internal class BundleResult
    {
        public BundleDefinition Definition { get; set; }
        public IEnumerable<IClientDependencyFile> Files { get; set; }
    }
}