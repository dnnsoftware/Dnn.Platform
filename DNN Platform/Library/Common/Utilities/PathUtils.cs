#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Common.Utilities
{
    public class PathUtils : ComponentBase<IPathUtils, PathUtils>, IPathUtils
    {
        private static readonly Regex FolderPathRx = new Regex("^0\\\\", RegexOptions.Compiled);

        #region Constructor

        internal PathUtils()
        {
        }

        #endregion

        #region Public Enums

        public enum UserFolderElement
        {
            Root = 0,
            SubFolder = 1
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds backslash to the specified source.
        /// </summary>
        /// <param name="source">The source string to be modified.</param>
        /// <returns>The original string plus a backslash.</returns>
        public virtual string AddTrailingSlash(string source)
        {
            Requires.PropertyNotNull("source", source);

            return source.EndsWith("\\") ? source : source + "\\";
        }

        /// <summary>
        /// Formats the provided folder path by adding a slash if needed.
        /// </summary>
        /// <param name="folderPath">The folder path to format.</param>
        /// <returns>The formatted path.</returns>
        public virtual string FormatFolderPath(string folderPath)
        {
            //Can not call trim on folderpath since folder passed in might have a legit space
            //at the begingin of its name " MyFolder/Test" is not same physical folder as "MyFoler/Test" 
            if (String.IsNullOrEmpty(folderPath) || String.IsNullOrEmpty(folderPath.Trim()))
            {
                return "";
            }

			return folderPath.EndsWith("/") ? folderPath.Trim() : folderPath.Trim() + "/";
        }

        /// <summary>
        /// Gets the physical path for the specified relative path.
        /// </summary>
        public virtual string GetPhysicalPath(int portalID, string relativePath)
        {
            Requires.PropertyNotNull("relativePath", relativePath);

            var path1 = GetRootFolderMapPath(portalID);
            var path2 = relativePath.Replace("/", "\\");

            if (Path.IsPathRooted(path2))
            {
                path2 = path2.TrimStart(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            }

            var physicalPath = Path.Combine(path1, path2);

            return RemoveTrailingSlash(physicalPath);
        }

        /// <summary>
        /// Gets the relative path for the specified physical path.
        /// </summary>
        public virtual string GetRelativePath(int portalID, string physicalPath)
        {
            Requires.PropertyNotNull("physicalPath", physicalPath);

            if (!Directory.Exists(physicalPath))
            {
                throw new ArgumentException("The argument 'physicalPath' is not a valid path. " + physicalPath);
            }

            var rootFolderMapPath = RemoveTrailingSlash(GetRootFolderMapPath(portalID));

            string relativePath;

            if (physicalPath.StartsWith(rootFolderMapPath, StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = physicalPath.Substring(rootFolderMapPath.Length);

                if (Path.IsPathRooted(relativePath))
                {
                    relativePath = relativePath.TrimStart(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
                }

                relativePath = relativePath.Replace("\\", "/");
            }
            else
            {
                throw new ArgumentException("The argument 'physicalPath' is not a valid path.");
            }

            return FormatFolderPath(relativePath);
        }

        /// <summary>
        /// Gets the physical root folder path for the specified portal
        /// </summary>
        public virtual string GetRootFolderMapPath(int portalID)
        {
            return (portalID == Null.NullInteger) ? GetHostMapPath() : GetPortalMapPath(portalID);
        }

        /// <summary>
        /// Gets the path to a user folder.
        /// </summary>
        /// <param name="user">The user info.</param>
        /// <returns>The path to a user folder.</returns>
        public virtual string GetUserFolderPath(UserInfo user)
        {
            return FolderManager.Instance.GetUserFolder(user).FolderPath;
        }
        
        /// <summary>
        /// Get elements from the user folder path.
        /// </summary>
        /// <param name="userID">The user identifier.</param>
        /// <param name="mode">The UserFolderElement to get.</param>
        /// <returns>The element from the user folder path.</returns>
        public virtual string GetUserFolderPathElement(int userID, UserFolderElement mode)
        {
            return GetUserFolderPathElementInternal(userID, mode);
        }

        internal string GetUserFolderPathElementInternal(int userId, UserFolderElement mode)
        {
            const int subfolderSeedLength = 2;
            const int byteOffset = 255;
            var element = "";

            switch (mode)
            {
                case UserFolderElement.Root:
                    element = (Convert.ToInt32(userId) & byteOffset).ToString("000");
                    break;
                case UserFolderElement.SubFolder:
                    element = userId.ToString("00").Substring(userId.ToString("00").Length - subfolderSeedLength, subfolderSeedLength);
                    break;
            }

            return element;
        } 
        
        internal string GetUserFolderPathInternal(UserInfo user)
        {
            var rootFolder = GetUserFolderPathElementInternal(user.UserID, UserFolderElement.Root);
            var subFolder = GetUserFolderPathElementInternal(user.UserID, UserFolderElement.SubFolder);

            var fullPath = Path.Combine(Path.Combine(rootFolder, subFolder), user.UserID.ToString(CultureInfo.InvariantCulture));

            return String.Format("Users/{0}/", fullPath.Replace("\\", "/"));
        }

        /// <summary>
        /// Checks if a folder is a default protected folder.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <returns>True if the folderPath is a default protected folder. False otherwise.</returns>
        public virtual bool IsDefaultProtectedPath(string folderPath)
        {
            return String.IsNullOrEmpty(folderPath) ||
                   folderPath.ToLower() == "skins" ||
                   folderPath.ToLower() == "containers" ||
                   folderPath.ToLower().StartsWith("skins/") ||
                   folderPath.ToLower().StartsWith("containers/");
        }

        /// <summary>
        /// The MapPath method maps the specified relative or virtual path to the corresponding physical directory on the server.
        /// </summary>
        /// <param name="path">Specifies the relative or virtual path to map to a physical directory. If Path starts with either 
        /// a forward (/) or backward slash (\), the MapPath method returns a path as if Path were a full, virtual path. If Path 
        /// doesn't start with a slash, the MapPath method returns a path relative to the directory of the .asp file being processed</param>
        /// <returns></returns>
        /// <remarks>
        /// If path is a null reference (Nothing in Visual Basic), then the MapPath method returns the full physical path 
        /// of the directory that contains the current application
        /// </remarks>
        public virtual string MapPath(string path)
        {
            Requires.PropertyNotNull("path", path);

            var convertedPath = path;

            var applicationPath = Globals.ApplicationPath;
            var applicationMapPath = Globals.ApplicationMapPath;

            if (applicationPath.Length > 1 && convertedPath.StartsWith(applicationPath))
            {
                convertedPath = convertedPath.Substring(applicationPath.Length);
            }

            convertedPath = convertedPath.Replace("/", "\\");

            if (path.StartsWith("~") | path.StartsWith(".") | path.StartsWith("/"))
            {
                convertedPath = convertedPath.Length > 1 ? string.Concat(AddTrailingSlash(applicationMapPath), convertedPath.Substring(1)) : applicationMapPath;
            }

            convertedPath = Path.GetFullPath(convertedPath);

            if (!convertedPath.StartsWith(applicationMapPath))
            {
                throw new HttpException();
            }

            return convertedPath;
        }

        /// <summary>
        /// Removes the trailing slash or backslash from the specified source.
        /// </summary>
        /// <param name="source">The source string to be modified.</param>
        /// <returns>The original string minus the trailing slash.</returns>
        public virtual string RemoveTrailingSlash(string source)
        {
            if (String.IsNullOrEmpty(source))
            {
                return "";
            }

            if (source.EndsWith("\\") || source.EndsWith("/"))
            {
                return source.Substring(0, source.Length - 1);
            }

            return source;
        }

        /// <summary>
        /// Strips the original path by removing starting 0 or 0\\.
        /// </summary>
        /// <param name="originalPath">The original path.</param>
        /// <returns>The stripped path.</returns>
        public virtual string StripFolderPath(string originalPath)
        {
            Requires.PropertyNotNull("originalPath", originalPath);

            if (originalPath.IndexOf("\\", StringComparison.InvariantCulture) >= 0)
            {
                return FolderPathRx.Replace(originalPath, "");
            }

            return originalPath.StartsWith("0") ? originalPath.Substring(1) : originalPath;
        }

        #endregion

        #region Private Methods

        private static string GetHostMapPath()
        {
            return Globals.HostMapPath;
        }

        private static string GetPortalMapPath(int portalId)
        {
            var portalInfo = PortalController.Instance.GetPortal(portalId);
            return portalInfo.HomeDirectoryMapPath;
        }

        #endregion
    }
}
