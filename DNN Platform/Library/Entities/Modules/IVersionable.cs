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

namespace DotNetNuke.Entities.Modules
{
    /// <summary>
    /// This interface allow the page to interact with his modules to delete/rollback or publish a specific version. 
    /// The module that wants support page versioning need to implement it in the Bussiness controller.
    /// </summary>
    public interface IVersionable
    {
        /// <summary>
        /// This method deletes a concrete version of the module
        /// </summary>
        /// <param name="moduleId">ModuleId</param>
        /// <param name="version">Version number</param>
        void DeleteVersion(int moduleId, int version);

        /// <summary>
        /// This method performs a rollback of a concrete version of the module
        /// </summary>
        /// <param name="moduleId">Module Id</param>
        /// <param name="version">Version number that need to be rollback</param>
        /// <returns>New version number created after the rollback process</returns>
        int RollBackVersion(int moduleId, int version);

        /// <summary>
        /// This method publishes a version of the module
        /// </summary>
        /// <param name="moduleId">Module Id</param>
        /// <param name="version">Version number</param>
        void PublishVersion(int moduleId, int version);

        /// <summary>
        /// This method returns the version number of the current published module version
        /// </summary>
        /// <param name="moduleId">Module Id</param>
        /// <returns>Version number of the current published content version</returns>
        int GetPublishedVersion(int moduleId);

        /// <summary>
        /// This method returns the latest version number of the current module
        /// </summary>
        /// <param name="moduleId">Module Id</param>
        /// <returns>Version number of the current published content version</returns>
        int GetLatestVersion(int moduleId);
    }
}
