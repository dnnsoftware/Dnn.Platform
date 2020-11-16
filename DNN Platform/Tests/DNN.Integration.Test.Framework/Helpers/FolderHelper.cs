// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Helpers
{
    using System;
    using System.Globalization;
    using System.IO;

    public static class FolderHelper
    {
        private const int ByteOffset = 255;
        private const int SubfolderSeedLength = 2;

        public static string GetUserFolderPath(int userId)
        {
            var rootFolder = GetUserFolderPathRoot(userId);
            var subFolder = GetUserFolderPathSubFolder(userId);

            var fullPath = Path.Combine(Path.Combine(rootFolder, subFolder), userId.ToString(CultureInfo.InvariantCulture));

            return string.Format("Users/{0}/", fullPath.Replace("\\", "/"));
        }

        private static string GetUserFolderPathRoot(int userId)
        {
            return (Convert.ToInt32(userId) & ByteOffset).ToString("000");
        }

        private static string GetUserFolderPathSubFolder(int userId)
        {
            return userId.ToString("00").Substring(userId.ToString("00").Length - SubfolderSeedLength, SubfolderSeedLength);
        }
    }
}
