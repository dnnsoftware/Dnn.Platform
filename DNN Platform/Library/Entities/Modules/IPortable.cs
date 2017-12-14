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
namespace DotNetNuke.Entities.Modules
{
    /// <summary>A contract specifying the ability to import and export the content of a module</summary>
    public interface IPortable
    {
        /// <summary>Exports the content of this module</summary>
        /// <param name="ModuleID">The ID of the module to export</param>
        /// <returns>The module's content serialized as a <see cref="string"/></returns>
        string ExportModule(int ModuleID);

        /// <summary>Imports the content of a module</summary>
        /// <param name="ModuleID">The ID of the module into which the content is being imported</param>
        /// <param name="Content">The content to import</param>
        /// <param name="Version">The version of the module from which the content is coming</param>
        /// <param name="UserID">The ID of the user performing the import</param>
        void ImportModule(int ModuleID, string Content, string Version, int UserID);
    }
}