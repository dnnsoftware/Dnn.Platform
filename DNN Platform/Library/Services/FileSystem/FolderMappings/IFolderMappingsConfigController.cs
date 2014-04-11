using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetNuke.Services.FileSystem.FolderMappings
{
    public interface IFolderMappingsConfigController
    {
        /// <summary>
        /// All folder types read from folderMappings config file
        /// </summary>
        IList<FolderTypeConfig> FolderTypes { get; }

        /// <summary>
        /// All folder mappings read from folderMappings config file
        /// </summary>
        IDictionary<string, string> FolderMappings { get;}

        /// <summary>
        /// Root node in folderMappings config file
        /// </summary>
        string ConfigNode { get; }
        
        /// <summary>
        /// Load data from folderMappings config file
        /// </summary>
        void LoadConfig();

        /// <summary>
        /// Save data in folderMappings config file
        /// </summary>
        /// <param name="folderMappinsSettings"></param>
        void SaveConfig(string folderMappinsSettings);
    }
}
