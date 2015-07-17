using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ClientDependency.Core.Config
{
    public class CompositeFileSection : ConfigurationElement
    {

        /// <summary>
        /// All of the file processing providers registered
        /// </summary>
        [ConfigurationProperty("fileProcessingProviders")]
        public ProviderSettingsCollection FileProcessingProviders
        {
            get
            {
                //We need to check for legacy settings here:
                var collection = (ProviderSettingsCollection)base["fileProcessingProviders"];
                var legacyCollection = (ProviderSettingsCollection)base["providers"];
                if (collection.Count == 0 && legacyCollection.Count > 0)
                {
                    //need to return the legacy collection
                    return legacyCollection;
                }

                return collection;
            }
        }

        [Obsolete("Use FileProcessingProviders instead")]
        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection FileProcessingProvidersLegacy
        {
            get { return (ProviderSettingsCollection)base["providers"]; }
        }

        /// <summary>
        /// All of the File map providers registered
        /// </summary>
        [ConfigurationProperty("fileMapProviders")]
        public ProviderSettingsCollection FileMapProviders
        {
            get { return (ProviderSettingsCollection)base["fileMapProviders"]; }
        }

        /// <summary>
        /// The default File processing provider
        /// </summary>
        [StringValidator(MinLength = 1)]
        [ConfigurationProperty("defaultFileProcessingProvider", DefaultValue = "CompositeFileProcessor")]
        public string DefaultFileProcessingProvider
        {
            get { return (string)base["defaultFileProcessingProvider"]; }
            set { base["defaultFileProcessingProvider"] = value; }
        }

        [Obsolete("Use DefaultFileProcessingProvider instead")]
        [ConfigurationProperty("defaultProvider", DefaultValue = "CompositeFileProcessor")]
        public string DefaultFileProcessingProviderLegacy
        {
            get
            {
                return (string)base["defaultProvider"];
            }
            set
            {
                base["defaultProvider"] = value;
            }
        }

        /// <summary>
        /// The default file map provider
        /// </summary>
        [StringValidator(MinLength = 1)]
        [ConfigurationProperty("defaultFileMapProvider", DefaultValue = "XmlFileMap")]
        public string DefaultFileMapProvider
        {
            get { return (string)base["defaultFileMapProvider"]; }
            set { base["defaultFileMapProvider"] = value; }
        }

        [ConfigurationProperty("compositeFileHandlerPath", DefaultValue = "DependencyHandler.axd")]
        public string CompositeFileHandlerPath
        {
            get { return (string)base["compositeFileHandlerPath"]; }
            set { base["compositeFileHandlerPath"] = value; }
        }

        [ConfigurationProperty("mimeTypeCompression")]
        public MimeTypeCompressionCollection MimeTypeCompression
        {
            get
            {
                return (MimeTypeCompressionCollection)base["mimeTypeCompression"];
            }
        }

        [ConfigurationProperty("rogueFileCompression")]
        public RogueFileCompressionCollection RogueFileCompression
        {
            get
            {
                return (RogueFileCompressionCollection)base["rogueFileCompression"];
            }
        }

       
    }
}
