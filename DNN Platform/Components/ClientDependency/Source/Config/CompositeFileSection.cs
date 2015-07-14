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
            get { return (ProviderSettingsCollection)base["fileProcessingProviders"]; }
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
