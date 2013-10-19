#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

namespace DotNetNuke.Services.FileSystem.Internal
{
    public interface IUserSecurityController
    {
        /// <summary>
        /// Checks if the Current user is Host user or Admin user of the provided portal
        /// </summary>
        /// <param name="portalId">Portal Id to check Admin users</param>
        /// <returns>True if the Current user is Host user or Admin user. False otherwise</returns>
        bool IsHostAdminUser(int portalId);

        /// <summary>
        /// Checks if the provided user is Host user or Admin user of the provided portal
        /// </summary>
        /// <param name="portalId">Portal Id to check Admin users</param>
        /// <param name="userId">User Id to check</param>
        /// <returns>True if the user is Host user or Admin user. False otherwise</returns>
        bool IsHostAdminUser(int portalId, int userId);

        /// <summary>
        /// Checks if the provided permission is allowed for the current user in the provided folder
        /// </summary>
        /// <param name="folder">Folder to check</param>
        /// <param name="permissionKey">Permission key to check</param>
        /// <returns></returns>
        bool HasFolderPermission(IFolderInfo folder, string permissionKey);
    }
}