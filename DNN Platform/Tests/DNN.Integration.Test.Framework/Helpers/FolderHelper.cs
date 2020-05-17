﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Globalization;
using System.IO;

namespace DNN.Integration.Test.Framework.Helpers
{
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
