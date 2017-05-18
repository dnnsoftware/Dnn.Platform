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

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Personalization;
using DotNetNuke.Services.Tokens;
using DotNetNuke.UI.Skins;

#endregion

namespace DotNetNuke.Entities.Portals
{
	public partial class PortalSettings
	{
		private ArrayList _desktopTabs;
        private string _version;

		[Obsolete("Deprecated in DNN 5.0. Replaced by DefaultAdminContainer")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public SkinInfo AdminContainer { get; set; }

		[Obsolete("Deprecated in DNN 5.0. Replaced by DefaultAdminSkin")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public SkinInfo AdminSkin { get; set; }

		[Obsolete("Deprecated in DNN 5.0. Replaced by Host.GetHostSettingsDictionary")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Hashtable HostSettings
		{
			get
			{
				var h = new Hashtable();
				foreach (ConfigurationSetting kvp in HostController.Instance.GetSettings().Values)
				{
					h.Add(kvp.Key, kvp.Value);
				}
				return h;
			}
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by extended UserMode property.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ContentVisible
		{
			get
			{
				return UserMode != Mode.Layout;
			}
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by DefaultPortalContainer")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public SkinInfo PortalContainer { get; set; }

		[Obsolete("Deprecated in DNN 5.0. Replaced by DefaultPortalSkin")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public SkinInfo PortalSkin { get; set; }

		[Obsolete("Deprecated in DNN 5.0. Tabs are cached independeently of Portal Settings, and this property is thus redundant")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ArrayList DesktopTabs
		{
			get
			{
				if (_desktopTabs == null)
				{
					_desktopTabs = new ArrayList();

					//Add each portal Tab to DesktopTabs
					TabInfo objPortalTab;
					foreach (TabInfo objTab in TabController.GetTabsBySortOrder(PortalId, CultureCode, true))
					{
						// clone the tab object ( to avoid creating an object reference to the data cache )
						objPortalTab = objTab.Clone();

						// set custom properties
						if (objPortalTab.TabOrder == 0)
						{
							objPortalTab.TabOrder = 999;
						}
						if (Null.IsNull(objPortalTab.StartDate))
						{
							objPortalTab.StartDate = DateTime.MinValue;
						}
						if (Null.IsNull(objPortalTab.EndDate))
						{
							objPortalTab.EndDate = DateTime.MaxValue;
						}

						_desktopTabs.Add(objPortalTab);
					}

					//Add each host Tab to DesktopTabs
					TabInfo objHostTab;
					foreach (TabInfo objTab in TabController.GetTabsBySortOrder(Null.NullInteger, Null.NullString, true))
					{
						// clone the tab object ( to avoid creating an object reference to the data cache )
						objHostTab = objTab.Clone();
						objHostTab.PortalID = PortalId;
						objHostTab.StartDate = DateTime.MinValue;
						objHostTab.EndDate = DateTime.MaxValue;

						_desktopTabs.Add(objHostTab);
					}
				}

				return _desktopTabs;
			}
		}

		[Obsolete("Deprecated in DNN 5.1. Replaced by Application.Version")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string Version
		{
			get
			{
				if (string.IsNullOrEmpty(_version))
				{
					_version = DotNetNukeContext.Current.Application.Version.ToString(3);
				}
				return _version;
			}
			set
			{
				_version = value;
			}
		}

		[Obsolete("Deprecated in DNN 6.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int TimeZoneOffset
		{
			get
			{
				return Convert.ToInt32(TimeZone.BaseUtcOffset.TotalMinutes);
			}
			set
			{
				TimeZone = Localization.ConvertLegacyTimeZoneOffsetToTimeZoneInfo(value);
			}
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by DataProvider.ExecuteScript")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static string ExecuteScript(string strScript)
		{
			return DataProvider.Instance().ExecuteScript(strScript);
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by DataProvider.ExecuteScript")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static string ExecuteScript(string strScript, bool useTransactions)
		{
			return DataProvider.Instance().ExecuteScript(strScript);
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by Globals.FindDatabaseVersion")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool FindDatabaseVersion(int major, int minor, int build)
		{
			return Globals.FindDatabaseVersion(major, minor, build);
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by DataProvider.GetDatabaseVersion")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IDataReader GetDatabaseVersion()
		{
			return DataProvider.Instance().GetDatabaseVersion();
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by Host.GetHostSettingsDictionary")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Hashtable GetHostSettings()
		{
			var h = new Hashtable();
			foreach (KeyValuePair<string, string> kvp in HostController.Instance.GetSettingsDictionary())
			{
				h.Add(kvp.Key, kvp.Value);
			}
			return h;
		}

		[Obsolete("Deprecated in DNN 5.0.  Please use ModuleController.GetModuleSettings(ModuleId)")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Hashtable GetModuleSettings(int moduleId)
		{
			return new ModuleController().GetModuleSettings(moduleId);
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by PortalAliasController.GetPortalAliasInfo")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static PortalAliasInfo GetPortalAliasInfo(string portalAlias)
		{
			return PortalAliasController.Instance.GetPortalAlias(portalAlias);
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by PortalAliasController.GetPortalAliasByPortal")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static string GetPortalByID(int portalId, string portalAlias)
		{
			return PortalAliasController.GetPortalAliasByPortal(portalId, portalAlias);
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by PortalAliasController.GetPortalAliasByTab")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static string GetPortalByTab(int tabID, string portalAlias)
		{
			return PortalAliasController.GetPortalAliasByTab(tabID, portalAlias);
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by PortalAliasController.GetPortalAliasLookup")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static PortalAliasCollection GetPortalAliasLookup()
		{
			var portalAliasCollection = new PortalAliasCollection();
			var aliasController = new PortalAliasController();
			foreach (var kvp in aliasController.GetPortalAliasesInternal())
			{
				portalAliasCollection.Add(kvp.Key, kvp.Value);
			}

			return portalAliasCollection;
		}

        [Obsolete("Deprecated in DNN 7.4. Replaced by PortalSettingsController.Instance().GetPortalAliasMappingMode")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static PortalAliasMapping GetPortalAliasMappingMode(int portalId)
        {
            return PortalSettingsController.Instance().GetPortalAliasMappingMode(portalId);
        }

		[Obsolete("Deprecated in DNN 5.0. Replaced by DataProvider.GetProviderPath")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static string GetProviderPath()
		{
			return DataProvider.Instance().GetProviderPath();
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by PortalController.GetPortalSettingsDictionary")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Hashtable GetSiteSettings(int portalId)
		{
			var h = new Hashtable();
            foreach (KeyValuePair<string, string> kvp in PortalController.Instance.GetPortalSettings(portalId))
			{
				h.Add(kvp.Key, kvp.Value);
			}
			return h;
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by PortalController.GetPortalSettingsDictionary(portalId).TryGetValue(settingName) or for the most part by proeprties of PortalSettings")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static string GetSiteSetting(int portalId, string settingName)
		{
			string setting;
            PortalController.Instance.GetPortalSettings(portalId).TryGetValue(settingName, out setting);
			return setting;
		}

		[Obsolete("Deprecated in DNN 5.0.  Please use ModuleController.GetTabModuleSettings(TabModuleId)")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Hashtable GetTabModuleSettings(int tabModuleId)
		{
			return new ModuleController().GetTabModuleSettings(tabModuleId);
		}

		[Obsolete("Deprecated in DNN 5.0.  Please use ModuleController.GetTabModuleSettings(ModuleId)")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Hashtable GetTabModuleSettings(int tabModuleId, Hashtable moduleSettings)
		{
			Hashtable tabModuleSettings = new ModuleController().GetTabModuleSettings(tabModuleId);

			// add the TabModuleSettings to the ModuleSettings
			foreach (string strKey in tabModuleSettings.Keys)
			{
				moduleSettings[strKey] = tabModuleSettings[strKey];
			}

			return moduleSettings;
		}

		[Obsolete("Deprecated in DNN 5.0.  Please use ModuleController.GetTabModuleSettings(ModuleId)")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Hashtable GetTabModuleSettings(Hashtable moduleSettings, Hashtable tabModuleSettings)
		{
			// add the TabModuleSettings to the ModuleSettings
			foreach (string strKey in tabModuleSettings.Keys)
			{
				moduleSettings[strKey] = tabModuleSettings[strKey];
			}

			//Return the modifed ModuleSettings
			return moduleSettings;
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by DataProvider.UpdateDatabaseVersion")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void UpdateDatabaseVersion(int major, int minor, int build)
		{
			DataProvider.Instance().UpdateDatabaseVersion(major, minor, build, DotNetNukeContext.Current.Application.Name);
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by DataProvider.UpdatePortalSetting(Integer, String, String)")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void UpdatePortalSetting(int portalId, string settingName, string settingValue)
		{
			PortalController.UpdatePortalSetting(portalId, settingName, settingValue);
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by PortalController.UpdatePortalSetting(Integer, String, String)")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void UpdateSiteSetting(int portalId, string settingName, string settingValue)
		{
			PortalController.UpdatePortalSetting(portalId, settingName, settingValue);
		}
	}
}
