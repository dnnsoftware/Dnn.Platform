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
using System.Collections;

using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Services.FileSystem
{
    public abstract class FolderMappingSettingsControlBase : PortalModuleBase
    {
        /// <summary>
        /// Use this method to load the provider's concrete settings.
        /// </summary>
        /// <param name="folderMappingSettings">The Hashtable containing the folder mapping settings.</param>
        /// <example>
        /// public override void LoadSettings(Hashtable folderMappingSettings)
        /// {
        ///     if (folderMappingSettings.ContainsKey("AccessKeyId"))
        ///     {
        ///         tbSettingValue.Text = folderMappingSettings["SettingName"].ToString();
        ///     }
        /// }
        /// </example>
        public abstract void LoadSettings(Hashtable folderMappingSettings);
        
        /// <summary>
        /// Use this method to update the provider's concrete settings for the specified folder mapping.
        /// </summary>
        /// <param name="folderMappingID">The folder mapping identifier.</param>
        /// <remarks>
        /// Because this method is executed after adding / updating the folder mapping, if there are validation errors,
        /// please throw an exception, as can be seen in the provided example.
        /// </remarks>
        /// <example>
        /// public override void UpdateSettings(int folderMappingID)
        /// {
        ///     Page.Validate();
        ///
        ///     if (Page.IsValid)
        ///     {
        ///         var folderMappingController = FolderMappingController.Instance;
        ///         var folderMapping = folderMappingController.GetFolderMapping(folderMappingID);
        ///
        ///         folderMapping.FolderMappingSettings["SettingName"] = tbSettingValue.Text;
        ///
        ///         folderMappingController.UpdateFolderMapping(folderMapping);
        ///     }
        ///     else
        ///     {
        ///         throw new Exception();
        ///     }
        /// }
        /// </example>
        public abstract void UpdateSettings(int folderMappingID);
    }
}