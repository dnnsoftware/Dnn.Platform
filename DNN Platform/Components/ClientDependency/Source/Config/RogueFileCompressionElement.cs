using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ClientDependency.Core.Config
{
    class RogueFileCompressionElement : ConfigurationElement
    {
        [ConfigurationProperty("compressJs", DefaultValue = true)]
        public bool CompressJs
        {
            get
            {
                return (bool)this["compressJs"];
            }
        }

        [ConfigurationProperty("compressCss", DefaultValue = true)]
        public bool CompressCss
        {
            get
            {
                return (bool)this["compressCss"];
            }
        }

        [ConfigurationProperty("path", DefaultValue = "*")]
        public string FilePath
        {
            get
            {
                return (string)this["path"];
            }
        }

        /// <summary>
        /// a collection of file extensions that must match on the rogue file for it to 
        /// be replaced with the composite handler
        /// </summary>
        [ConfigurationProperty("jsExt", DefaultValue = ".js")]
        public string JsRequestExtension
        {
            get
            {
                return (string)this["jsExt"];
            }
        }

        /// <summary>
        /// a collection of file extensions that must match on the rogue file for it to 
        /// be replaced with the composite handler
        /// </summary>
        [ConfigurationProperty("cssExt", DefaultValue = ".css")]
        public string CssRequestExtension
        {
            get
            {
                return (string)this["cssExt"];
            }
        }

        [ConfigurationProperty("exclusions")]
        public RogueFileCompressionExcludeCollection ExcludePaths
        {
            get
            {
                return (RogueFileCompressionExcludeCollection)base["exclusions"];
            }
        }       

        public override int GetHashCode()
        {
            return this.FilePath.GetHashCode();
        }

    }
}
