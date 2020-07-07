// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System.Collections;
    using System.Collections.Generic;

    public interface IFolderMappingController
    {
        void AddDefaultFolderTypes(int portalId);

        int AddFolderMapping(FolderMappingInfo objFolderMapping);

        void DeleteFolderMapping(int portalId, int folderMappingId);

        FolderMappingInfo GetDefaultFolderMapping(int portalId);

        FolderMappingInfo GetFolderMapping(int folderMappingId);

        FolderMappingInfo GetFolderMapping(int portalId, int folderMappingId);

        FolderMappingInfo GetFolderMapping(int portalId, string mappingName);

        List<FolderMappingInfo> GetFolderMappings(int portalId);

        Hashtable GetFolderMappingSettings(int folderMappingId);

        void UpdateFolderMapping(FolderMappingInfo objFolderMapping);
    }
}
