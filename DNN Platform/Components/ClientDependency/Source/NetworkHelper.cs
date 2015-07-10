using System;
using System.Linq;
using ClientDependency.Core.Config;

namespace ClientDependency.Core
{
    public class NetworkHelper
    {
        /// <summary>
        /// Generally used for unit tests to get access to the settings
        /// </summary>
        internal static Func<ClientDependencySection> GetConfigSection;

        

        /// <summary>
        /// Returns the machine name that is safe to use in file paths.
        /// </summary>
        /// <remarks>
        /// see: https://github.com/Shandem/ClientDependency/issues/4
        /// </remarks>
        public static string FileSafeMachineName 
        {
            get { return MachineName.ReplaceNonAlphanumericChars('-'); }
        }

        /// <summary>
        /// Returns the current machine name
        /// </summary>
        /// <remarks>
        /// Tries to resolve the machine name, if it cannot it uses the config section.
        /// </remarks>
        public static string MachineName
        {
            get
            {
                var section = GetConfigSection == null
                                  ? ClientDependencySettings.GetDefaultSection()
                                  : GetConfigSection();

                if (!string.IsNullOrEmpty(section.MachineName))
                {
                    //return the config specified machine name
                    return section.MachineName;
                }

                try
                {
                    return Environment.MachineName;
                }
                catch
                {
                    try
                    {
                        return System.Net.Dns.GetHostName();
                    }
                    catch
                    {
                        //if we get here it means we cannot access the machine name
                        throw new ApplicationException("Cannot resolve the current machine name eithe by Environment.MachineName or by Dns.GetHostname(). Because of either security restrictions applied to this server or network issues not being able to resolve the hostname you will need to specify an explicity host name in the ClientDependency config section");
                    }
                }
            }
        }
    }
}