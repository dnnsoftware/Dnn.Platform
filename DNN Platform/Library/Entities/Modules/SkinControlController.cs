// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>ModuleControlController provides the Business Layer for Module Controls.</summary>
    public partial class SkinControlController
    {
        private static readonly DataProvider DataProvider = DataProvider.Instance();

        /// <summary>DeleteSkinControl deletes a Skin Control in the database.</summary>
        /// <param name="skinControl">The Skin Control to delete.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void DeleteSkinControl(SkinControlInfo skinControl)
            => DeleteSkinControl(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), skinControl);

        /// <summary>DeleteSkinControl deletes a Skin Control in the database.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="skinControl">The Skin Control to delete.</param>
        public static void DeleteSkinControl(IEventLogger eventLogger, SkinControlInfo skinControl)
        {
            DataProvider.DeleteSkinControl(skinControl.SkinControlID);
            eventLogger.AddLog(skinControl, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.SKINCONTROL_DELETED);
        }

        /// <summary>GetSkinControl gets a single Skin Control from the database.</summary>
        /// <param name="skinControlID">The ID of the SkinControl.</param>
        /// <returns>The <see cref="SkinControlInfo"/> or <see langword="null"/>.</returns>
        public static SkinControlInfo GetSkinControl(int skinControlID)
        {
            return CBO.FillObject<SkinControlInfo>(DataProvider.GetSkinControl(skinControlID));
        }

        /// <summary>GetSkinControlByPackageID gets a single Skin Control from the database.</summary>
        /// <param name="packageID">The ID of the Package.</param>
        /// <returns>The <see cref="SkinControlInfo"/> or <see langword="null"/>.</returns>
        public static SkinControlInfo GetSkinControlByPackageID(int packageID)
        {
            return CBO.FillObject<SkinControlInfo>(DataProvider.GetSkinControlByPackageID(packageID));
        }

        /// <summary>GetSkinControlByKey gets a single Skin Control from the database.</summary>
        /// <param name="key">The key of the Control.</param>
        /// <returns>The <see cref="SkinControlInfo"/> or <see langword="null"/>.</returns>
        public static SkinControlInfo GetSkinControlByKey(string key)
        {
            return CBO.FillObject<SkinControlInfo>(DataProvider.GetSkinControlByKey(key));
        }

        /// <summary>GetSkinControls gets all the Skin Controls from the database.</summary>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> mapping skin control ID to <see cref="SkinControlInfo"/>.</returns>
        public static Dictionary<string, SkinControlInfo> GetSkinControls()
        {
            return CBO.FillDictionary("ControlKey", DataProvider.GetSkinControls(), new Dictionary<string, SkinControlInfo>());
        }

        /// <summary>SaveSkinControl updates a Skin Control in the database.</summary>
        /// <param name="skinControl">The Skin Control to save.</param>
        /// <returns>The skin control ID.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial int SaveSkinControl(SkinControlInfo skinControl)
            => SaveSkinControl(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), skinControl);

        /// <summary>SaveSkinControl updates a Skin Control in the database.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="skinControl">The Skin Control to save.</param>
        /// <returns>The skin control ID.</returns>
        public static int SaveSkinControl(IEventLogger eventLogger, SkinControlInfo skinControl)
        {
            int skinControlID = skinControl.SkinControlID;
            if (skinControlID == Null.NullInteger)
            {
                // Add new Skin Control
                skinControlID = DataProvider.AddSkinControl(
                    skinControl.PackageID,
                    skinControl.ControlKey,
                    skinControl.ControlSrc,
                    skinControl.SupportsPartialRendering,
                    UserController.Instance.GetCurrentUserInfo().UserID);
                eventLogger.AddLog(skinControl, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.SKINCONTROL_CREATED);
            }
            else
            {
                // Upgrade Skin Control
                DataProvider.UpdateSkinControl(
                    skinControl.SkinControlID,
                    skinControl.PackageID,
                    skinControl.ControlKey,
                    skinControl.ControlSrc,
                    skinControl.SupportsPartialRendering,
                    UserController.Instance.GetCurrentUserInfo().UserID);
                eventLogger.AddLog(skinControl, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.SKINCONTROL_UPDATED);
            }

            return skinControlID;
        }
    }
}
