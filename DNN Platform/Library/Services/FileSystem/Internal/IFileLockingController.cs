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
    public interface IFileLockingController
    {
        /// <summary>
        /// Checks if a file is locked or not
        /// </summary>
        /// <param name="file">The file to be checked</param>
        /// <param name="lockReasonKey">The friendly reason why the file is locked</param>
        /// <returns>True if the file is locked, false in otherwise</returns>
        bool IsFileLocked(IFileInfo file, out string lockReasonKey);

        /// <summary>
        /// Checks if the file is out of Publish Period
        /// </summary>
        /// <param name="file">the file to be checked</param>
        /// <param name="portalId">The Portal Id where the file is contained</param>
        /// <param name="userId">The user Id who is accessing to the file</param>
        /// <returns>True if the file is out of publish period, false in otherwise. In anycase, True for admin or host users</returns>
        bool IsFileOutOfPublishPeriod(IFileInfo file, int portalId, int userId);
    }
}