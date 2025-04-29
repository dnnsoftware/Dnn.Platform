// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities;

using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;

public class PathUtils : ComponentBase<IPathUtils, PathUtils>, IPathUtils
{
    private static readonly Regex FolderPathRx = new Regex("^0\\\\", RegexOptions.Compiled);

    /// <summary>Initializes a new instance of the <see cref="PathUtils"/> class.</summary>
    internal PathUtils()
    {
    }

    public enum UserFolderElement
    {
        Root = 0,
        SubFolder = 1,
    }

    /// <inheritdoc />
    public virtual string AddTrailingSlash(string source)
    {
        Requires.PropertyNotNull("source", source);

        return source.EndsWith("\\") ? source : source + "\\";
    }

    /// <inheritdoc />
    public virtual string FormatFolderPath(string folderPath)
    {
        // Can not call trim on folderpath since folder passed in might have a legit space
        // at the begingin of its name " MyFolder/Test" is not same physical folder as "MyFoler/Test"
        if (string.IsNullOrEmpty(folderPath) || string.IsNullOrEmpty(folderPath.Trim()))
        {
            return string.Empty;
        }

        return folderPath.EndsWith("/") ? folderPath.Trim() : folderPath.Trim() + "/";
    }

    /// <inheritdoc />
    public virtual string GetPhysicalPath(int portalID, string relativePath)
    {
        Requires.PropertyNotNull("relativePath", relativePath);

        var path1 = this.GetRootFolderMapPath(portalID);
        var path2 = relativePath.Replace("/", "\\");

        if (Path.IsPathRooted(path2))
        {
            path2 = path2.TrimStart(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
        }

        var physicalPath = Path.Combine(path1, path2);

        return this.RemoveTrailingSlash(physicalPath);
    }

    /// <inheritdoc />
    public virtual string GetRelativePath(int portalID, string physicalPath)
    {
        Requires.PropertyNotNull("physicalPath", physicalPath);

        if (!Directory.Exists(physicalPath))
        {
            throw new ArgumentException("The argument 'physicalPath' is not a valid path. " + physicalPath);
        }

        var rootFolderMapPath = this.RemoveTrailingSlash(this.GetRootFolderMapPath(portalID));

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

        return this.FormatFolderPath(relativePath);
    }

    /// <inheritdoc />
    public virtual string GetRootFolderMapPath(int portalID)
    {
        return (portalID == Null.NullInteger) ? GetHostMapPath() : GetPortalMapPath(portalID);
    }

    /// <summary>Gets the path to a user folder.</summary>
    /// <param name="user">The user info.</param>
    /// <returns>The path to a user folder.</returns>
    public virtual string GetUserFolderPath(UserInfo user)
    {
        return FolderManager.Instance.GetUserFolder(user).FolderPath;
    }

    /// <inheritdoc />
    public virtual string GetUserFolderPathElement(int userID, UserFolderElement mode)
    {
        return this.GetUserFolderPathElementInternal(userID, mode);
    }

    /// <inheritdoc />
    public virtual bool IsDefaultProtectedPath(string folderPath)
    {
        return string.IsNullOrEmpty(folderPath) ||
               folderPath.Equals("skins", StringComparison.InvariantCultureIgnoreCase) ||
               folderPath.Equals("containers", StringComparison.InvariantCultureIgnoreCase) ||
               folderPath.StartsWith("skins/", StringComparison.InvariantCultureIgnoreCase) ||
               folderPath.StartsWith("containers/", StringComparison.InvariantCultureIgnoreCase);
    }

    /// <inheritdoc />
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
            convertedPath = convertedPath.Length > 1 ? string.Concat(this.AddTrailingSlash(applicationMapPath), convertedPath.Substring(1)) : applicationMapPath;
        }

        convertedPath = Path.GetFullPath(convertedPath);

        if (!convertedPath.StartsWith(applicationMapPath))
        {
            throw new HttpException();
        }

        return convertedPath;
    }

    /// <inheritdoc />
    public virtual string RemoveTrailingSlash(string source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return string.Empty;
        }

        if (source.EndsWith("\\") || source.EndsWith("/"))
        {
            return source.Substring(0, source.Length - 1);
        }

        return source;
    }

    /// <inheritdoc />
    public virtual string StripFolderPath(string originalPath)
    {
        Requires.PropertyNotNull("originalPath", originalPath);

        if (originalPath.IndexOf("\\", StringComparison.InvariantCulture) >= 0)
        {
            return FolderPathRx.Replace(originalPath, string.Empty);
        }

        return originalPath;
    }

    internal string GetUserFolderPathElementInternal(int userId, UserFolderElement mode)
    {
        const int subfolderSeedLength = 2;
        const int byteOffset = 255;
        var element = string.Empty;

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
        var rootFolder = this.GetUserFolderPathElementInternal(user.UserID, UserFolderElement.Root);
        var subFolder = this.GetUserFolderPathElementInternal(user.UserID, UserFolderElement.SubFolder);

        var fullPath = Path.Combine(Path.Combine(rootFolder, subFolder), user.UserID.ToString(CultureInfo.InvariantCulture));

        return string.Format("Users/{0}/", fullPath.Replace("\\", "/"));
    }

    private static string GetHostMapPath()
    {
        return Globals.HostMapPath;
    }

    private static string GetPortalMapPath(int portalId)
    {
        var portalInfo = PortalController.Instance.GetPortal(portalId);
        return portalInfo.HomeDirectoryMapPath;
    }
}
