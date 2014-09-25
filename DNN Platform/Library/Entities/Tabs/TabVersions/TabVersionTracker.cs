#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using System;
using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    public class TabVersionTracker: ServiceLocator<ITabVersionTracker, TabVersionTracker>, ITabVersionTracker
    {
        #region Public Methods
        public void TrackModuleAddition(int tabId, int createdByUserID, ModuleInfo module, int moduleVersion)
        {
            if (!IsTabVersioningEnabled())
            {
                return;
            }

            var unPublishedVersion = GetOrCreateUnPublishedTabVersion(tabId, createdByUserID);
            var tabVersionDetail = CreateNewTabVersionDetailObjectFromModule(unPublishedVersion.TabVersionId, module, moduleVersion, TabVersionDetailAction.Added);
            TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail, createdByUserID);
        }

        public void TrackModuleModification(int tabId, int createdByUserID, ModuleInfo module, int moduleVersion)
        {
            TrackModuleModification(tabId, createdByUserID, module.ModuleID, module.PaneName, module.ModuleOrder, moduleVersion);
        }

        public void TrackModuleModification(int tabId, int createdByUserID, int moduleId, string paneName, int moduleOrder, int moduleVersion)
        {
            if (!IsTabVersioningEnabled())
            {
                return;
            }

            var unPublishedVersion = GetOrCreateUnPublishedTabVersion(tabId, createdByUserID);
            var tabVersionDetail = CreateNewTabVersionDetailObjectFromModule(unPublishedVersion.TabVersionId, moduleId, paneName, moduleOrder, moduleVersion, TabVersionDetailAction.Modified);

            var existingTabVersionDetail = TabVersionDetailController.Instance.GetTabVersionDetails(unPublishedVersion.TabVersionId).SingleOrDefault(tvd => tvd.ModuleId == moduleId);
            if (existingTabVersionDetail != null)
            {
                tabVersionDetail.TabVersionDetailId = existingTabVersionDetail.TabVersionDetailId;
                if (moduleVersion == Null.NullInteger)
                {
                    tabVersionDetail.ModuleVersion = existingTabVersionDetail.ModuleVersion;
                }
            }

            TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail, createdByUserID);
        }

        public void TrackModuleDeletion(int tabId, int createdByUserID, ModuleInfo module, int moduleVersion)
        {
            if (!IsTabVersioningEnabled())
            {
                return;
            }

            var unPublishedVersion = GetOrCreateUnPublishedTabVersion(tabId, createdByUserID);
            
            var existingTabDetail = TabVersionDetailController.Instance.GetTabVersionDetails(unPublishedVersion.TabVersionId).SingleOrDefault(tvd => tvd.ModuleId == module.ModuleID);
            if (existingTabDetail != null)
            {
                TabVersionDetailController.Instance.DeleteTabVersionDetail(existingTabDetail.TabVersionId, existingTabDetail.TabVersionDetailId); 
            }
            else
            {  
                var tabVersionDetail = CreateNewTabVersionDetailObjectFromModule(unPublishedVersion.TabVersionId, module, moduleVersion, TabVersionDetailAction.Deleted);
                TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail, createdByUserID);
            }
        }
        #endregion

        #region Private Methods
        private TabVersion GetOrCreateUnPublishedTabVersion(int tabId, int createdByUserID)
        {
            var unPublishedVersion = TabVersionMaker.Instance.GetUnPublishedVersion(tabId);
            return unPublishedVersion == null ? 
                TabVersionMaker.Instance.CreateNewVersion(tabId, createdByUserID) : 
                TabVersionController.Instance.GetTabVersions(tabId).SingleOrDefault(tv => !tv.IsPublished);
        }

        private TabVersionDetail CreateNewTabVersionDetailObjectFromModule(int tabVersionId, ModuleInfo module, int moduleVersion, TabVersionDetailAction action)
        {
            return new TabVersionDetail
            {
                TabVersionDetailId = 0,
                TabVersionId = tabVersionId,
                ModuleId = module.ModuleID,
                ModuleVersion = moduleVersion,
                ModuleOrder = module.ModuleOrder,
                PaneName = module.PaneName,
                Action = action              
            };
        }

        private TabVersionDetail CreateNewTabVersionDetailObjectFromModule(int tabVersionId, int moduleId, string paneName, int moduleOrder, int moduleVersion, TabVersionDetailAction action)
        {
            return new TabVersionDetail
            {
                TabVersionDetailId = 0,
                TabVersionId = tabVersionId,
                ModuleId = moduleId,
                ModuleVersion = moduleVersion,
                ModuleOrder = moduleOrder,
                PaneName = paneName,
                Action = action
            };
        }
        
        private static bool IsTabVersioningEnabled()
        {
            var portalId = PortalSettings.Current == null ? Null.NullInteger : PortalSettings.Current.PortalId;
            return portalId != Null.NullInteger && TabVersionSettings.Instance.IsVersioningEnabled(portalId);
        }
        #endregion

        protected override Func<ITabVersionTracker> GetFactory()
        {
            return () => new TabVersionTracker();
        }
    }
}
