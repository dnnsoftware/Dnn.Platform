// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System.Collections;

    using DotNetNuke.Entities.Modules;

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
        /// }.
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
        /// }.
        /// </example>
        public abstract void UpdateSettings(int folderMappingID);
    }
}
