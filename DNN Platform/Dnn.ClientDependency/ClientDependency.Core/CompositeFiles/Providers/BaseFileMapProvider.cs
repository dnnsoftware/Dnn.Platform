using System.Collections.Generic;
using System.Configuration.Provider;
using System.IO;
using System.Web;

namespace ClientDependency.Core.CompositeFiles.Providers
{
    public abstract class BaseFileMapProvider : ProviderBase, IHttpProvider
    {
        
        /// <summary>
        /// Retreives the file map for the key/version/compression type specified
        /// </summary>
        /// <param name="fileKey"></param>
        /// <param name="version"></param>
        /// <param name="compression"></param>
        /// <returns></returns>
        public abstract CompositeFileMap GetCompositeFile(string fileKey, 
            int version, 
            string compression);

        /// <summary>
        /// Retreives the dependent file paths for the filekey/version (regardless of compression)
        /// </summary>
        /// <param name="fileKey"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public abstract IEnumerable<string> GetDependentFiles(string fileKey, int version);

        /// <summary>
        /// Creates a map for the version/compression type/dependent file listing
        /// </summary>
        /// <param name="fileKey"></param>
        /// <param name="compressionType"></param>
        /// <param name="dependentFiles"></param>
        /// <param name="compositeFile"></param>
        /// <param name="version"></param>
        public abstract void CreateUpdateMap(string fileKey, 
            string compressionType,
            IEnumerable<IClientDependencyFile> dependentFiles, 
            string compositeFile, 
            int version);

        /// <summary>
        /// Creates a new file map and file key for the dependent file list, this is used to create URLs with CompositeUrlType.MappedId 
        /// </summary>       
        public abstract string CreateNewMap(HttpContextBase http,
                                   IEnumerable<IClientDependencyFile> dependentFiles,
                                   int version);

        /// <summary>
        /// Runs initialization with an Http context, this occurs after the initial provider config initialization
        /// </summary>
        /// <param name="http"></param>
        public abstract void Initialize(System.Web.HttpContextBase http);


    }
}