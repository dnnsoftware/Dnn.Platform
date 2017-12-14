#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Entities.Tabs
{
    public interface ITabChangeTracker
    {
        /// <summary>
        /// Tracks a change when a module is added to a page
        /// </summary>
        /// <param name="module">Module which tracks the change</param>
        /// <param name="moduleVersion">Version number corresponding to the change</param>
        /// <param name="userId">User Id who provokes the change</param>        
        void TrackModuleAddition(ModuleInfo module, int moduleVersion, int userId);

        /// <summary>
        /// Tracks a change when a module is modified on a page
        /// </summary>
        /// <param name="module">Module which tracks the change</param>
        /// <param name="moduleVersion">Version number corresponding to the change</param>
        /// <param name="userId">User Id who provokes the change</param>
        void TrackModuleModification(ModuleInfo module, int moduleVersion, int userId);

        /// <summary>
        /// Tracks a change when a module is deleted from a page
        /// </summary>
        /// <param name="module">Module which tracks the change</param>
        /// <param name="moduleVersion">Version number corresponding to the change</param>
        /// <param name="userId">User Id who provokes the change</param>
        void TrackModuleDeletion(ModuleInfo module, int moduleVersion, int userId);

        /// <summary>
        /// Tracks a change when a module is copied from an exisitng page
        /// </summary>
        /// <param name="module">Module which tracks the change</param>
        /// <param name="moduleVersion">Version number corresponding to the change</param>
        /// <param name="originalTabId">Tab Id where the module originally is</param>
        /// <param name="userId">User Id who provokes the change</param>
        void TrackModuleCopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId);

        /// <summary>
        /// Tracks a change when a copied module is deleted from an exisitng page
        /// </summary>
        /// <param name="module">Module which tracks the change</param>
        /// <param name="moduleVersion">Version number corresponding to the change</param>
        /// <param name="originalTabId">Tab Id where the module originally is</param>       
        /// <param name="userId">User Id who provokes the change</param>
        void TrackModuleUncopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId);
    }
}
