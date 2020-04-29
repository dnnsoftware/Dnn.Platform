using System;
using System.ComponentModel;
using System.Reflection;
using DotNetNuke.Services.FileSystem;

namespace Dnn.Modules.ResourceManager.Components.Common
{
    public class Utils
    {
        public static string GetEnumDescription(Enum clickBehaviour)
        {
            FieldInfo fi = clickBehaviour.GetType().GetField(clickBehaviour.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes[0].Description;
        }

        public static int GetFolderGroupId(int folderId)
        {
            var folder = FolderManager.Instance.GetFolder(folderId);
            var folderPath = folder.DisplayPath;

            if (!folderPath.StartsWith(Constants.GroupFolderPathStart))
            {
                return -1;
            }

            var prefixLength = Constants.GroupFolderPathStart.Length;
            var folderGroupIdString = folderPath.Substring(prefixLength);
            folderGroupIdString = folderGroupIdString.Substring(0, folderGroupIdString.IndexOf("/"));

            int folderGroupId;
            if (!int.TryParse(folderGroupIdString, out folderGroupId))
            {
                return -1;
            }
            return folderGroupId;
        }
    }
}