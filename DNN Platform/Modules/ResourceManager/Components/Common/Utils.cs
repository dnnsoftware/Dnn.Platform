// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.ComponentModel;
using DotNetNuke.Services.FileSystem;

namespace Dnn.Modules.ResourceManager.Components.Common
{
    public class Utils
    {
        public static string GetEnumDescription(Enum enumValue)
        {
            var fi = enumValue.GetType().GetField(enumValue.ToString());
            var descriptionAttributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (descriptionAttributes.Length > 0)
                return descriptionAttributes[0].Description;
            return enumValue.ToString();
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