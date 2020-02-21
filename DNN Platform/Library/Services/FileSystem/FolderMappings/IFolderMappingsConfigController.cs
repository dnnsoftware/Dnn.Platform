// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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

        /// <summary>
        /// Gets the folderMapping configured for a specific folder
        /// </summary>
        /// <param name="portalId">Portal Id where the folder is</param>
        /// <param name="folderPath">Specific folder path</param>        
        FolderMappingInfo GetFolderMapping(int portalId, string folderPath);
    }
}
