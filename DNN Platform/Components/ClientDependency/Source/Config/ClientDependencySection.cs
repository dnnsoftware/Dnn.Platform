using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Linq;
using ClientDependency.Core.Logging;

namespace ClientDependency.Core.Config
{
    public class ClientDependencySection : ConfigurationSection
	{
        /// <summary>
        /// Set the version for the files, this will reset all composite file caching, and if
        /// composite files are disabled will add a query string to each request so that 
        /// any client side cached files will be re-downloaded.
        /// </summary>
        [ConfigurationProperty("version", DefaultValue = 0)]
        public int Version
        {
            get { return (int)base["version"]; }
            set { base["version"] = value; }
        }
       
		[ConfigurationProperty("compositeFiles")]
		public CompositeFileSection CompositeFileElement
		{
		    get
		    {
				return (CompositeFileSection)this["compositeFiles"];
		    }
		}

		[ConfigurationProperty("fileRegistration")]
		public FileRegistrationSection FileRegistrationElement
		{
			get
			{
				return (FileRegistrationSection)this["fileRegistration"];
			}
		}

        [ConfigurationProperty("mvc")]
        public MvcSection MvcElement
        {
            get
            {
                return (MvcSection)this["mvc"];
            }
        }

        [ConfigurationProperty("loggerType")]
        public string LoggerType
        {
            get
            {
                return (string)this["loggerType"];
            }
        }

        /// <summary>
        /// Not really supposed to be used by public, but can implement at your own risk!
        /// This by default assigns the MvcFilter and RogueFileFilter.
        /// </summary>
        [ConfigurationProperty("filters", IsRequired = false)]
        public ProviderSettingsCollection Filters
        {
            get
            {
                var obj = base["filters"];

                if (obj == null || ((obj is ConfigurationElementCollection) && ((ConfigurationElementCollection)obj).Count == 0))
                {
                    var col = new ProviderSettingsCollection();
                    col.Add(new ProviderSettings("MvcFilter", "ClientDependency.Core.Mvc.MvcFilter, ClientDependency.Core.Mvc"));
                    col.Add(new ProviderSettings("RogueFileFilter", "ClientDependency.Core.Module.RogueFileFilter, ClientDependency.Core"));                    
                    return col;
                }
                else
                {
                    return (ProviderSettingsCollection)obj;
                }
            }
        }

        /// <summary>
        /// The configuration section to set the FileBasedDependencyExtensionList. This is a comma separated list.
        /// </summary>
        /// <remarks>
        /// If this is not explicitly set, then the extensions 'js' and 'css' are the defaults.
        /// </remarks>
        [ConfigurationProperty("fileDependencyExtensions", DefaultValue = ".js,.css")]
        protected string FileBasedDepdendenyExtensions
        {
            get { return (string)base["fileDependencyExtensions"]; }
            set { base["fileDependencyExtensions"] = value; }
        }

        /// <summary>
        /// The file extensions of Client Dependencies that are file based as opposed to request based.
        /// Any file that doesn't have the extensions listed here will be request based, request based is
        /// more overhead for the server to process.
        /// </summary>
        /// <example>
        /// A request based JavaScript file may be  a .ashx that dynamically creates JavaScript server side.
        /// Or an asmx/js request based on the proxied javascript created by web services.
        /// </example>
        /// <remarks>
        /// If this is not explicitly set, then the extensions 'js' and 'css' are the defaults.
        /// </remarks>
        public IEnumerable<string> FileBasedDependencyExtensionList
        {
            get
            {
                return FileBasedDepdendenyExtensions.Split(',')
                    .Select(x => x.Trim().ToUpper());
            }
        }
	}

}
