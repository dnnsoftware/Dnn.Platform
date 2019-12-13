using System.Collections;
using System.Collections.Generic;

namespace DotNetNuke.Services.FileSystem
{
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
