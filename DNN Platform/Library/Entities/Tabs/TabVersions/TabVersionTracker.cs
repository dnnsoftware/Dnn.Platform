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
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    public class TabVersionTracker: ServiceLocator<ITabVersionTracker, TabVersionTracker>, ITabVersionTracker
    {
        #region Public Methods
        public void TrackModuleAddition(ModuleInfo module, int moduleVersion, int userId)
        {
            Requires.NotNull("module", module);

            try
            {
                if (!IsVersioningEnabled(module))
                {
                    return;
                }

                var unPublishedVersion = GetOrCreateUnPublishedTabVersion(module.TabID, userId);
                var tabVersionDetail = CreateNewTabVersionDetailObjectFromModule(unPublishedVersion.TabVersionId, module,
                    moduleVersion, TabVersionDetailAction.Added);
                TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail, userId);
            }
            catch (Exception ex)
            {
                Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        public void TrackModuleModification(ModuleInfo module, int moduleVersion, int userId)
        {
            Requires.NotNull("module", module);

            try
            {
                if (!IsVersioningEnabled(module))
                {
                    return;
                }

                var unPublishedVersion = GetOrCreateUnPublishedTabVersion(module.TabID, userId);
                var tabVersionDetail = CreateNewTabVersionDetailObjectFromModule(unPublishedVersion.TabVersionId, module.ModuleID, module.PaneName, module.ModuleOrder, moduleVersion, TabVersionDetailAction.Modified);

                var existingTabVersionDetail = TabVersionDetailController.Instance.GetTabVersionDetails(unPublishedVersion.TabVersionId).SingleOrDefault(tvd => tvd.ModuleId == module.ModuleID);
                if (existingTabVersionDetail != null)
                {
                    tabVersionDetail.TabVersionDetailId = existingTabVersionDetail.TabVersionDetailId;
                    if (moduleVersion == Null.NullInteger)
                    {
                        tabVersionDetail.ModuleVersion = existingTabVersionDetail.ModuleVersion;
                    }

                    // If the operation is done over a just created module, the Added operation is kept.
                    if (existingTabVersionDetail.Action == TabVersionDetailAction.Added)
                    {
                        tabVersionDetail.Action = TabVersionDetailAction.Added;
                    }
                }

                TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail, userId);
            }
            catch (Exception ex)
            {
                Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        public void TrackModuleDeletion(ModuleInfo module, int moduleVersion, int userId)
        {
            Requires.NotNull("module", module);
            
            try
            {
                if (!IsVersioningEnabled(module))
                {
                    return;
                }

                var unPublishedVersion = GetOrCreateUnPublishedTabVersion(module.TabID, userId);
            
                var existingTabDetail = TabVersionDetailController.Instance.GetTabVersionDetails(unPublishedVersion.TabVersionId).SingleOrDefault(tvd => tvd.ModuleId == module.ModuleID);
                if (existingTabDetail != null)
                {
                    TabVersionDetailController.Instance.DeleteTabVersionDetail(existingTabDetail.TabVersionId, existingTabDetail.TabVersionDetailId); 
                }
                else
                {  
                    var tabVersionDetail = CreateNewTabVersionDetailObjectFromModule(unPublishedVersion.TabVersionId, module, moduleVersion, TabVersionDetailAction.Deleted);
                    TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail, userId);
                }
            }
            catch (Exception ex)
            {
                Services.Exceptions.Exceptions.LogException(ex);
            }
        }
        #endregion

        #region Private Statics Methods
        private static bool IsVersioningEnabled(ModuleInfo module)
        {
            return module.PortalID != Null.NullInteger && TabVersionSettings.Instance.IsVersioningEnabled(module.PortalID);
        }

        private static TabVersion GetOrCreateUnPublishedTabVersion(int tabId, int createdByUserID)
        {
            var unPublishedVersion = TabVersionMaker.Instance.GetUnPublishedVersion(tabId);
            return unPublishedVersion == null ? 
                TabVersionMaker.Instance.CreateNewVersion(tabId, createdByUserID) : 
                TabVersionController.Instance.GetTabVersions(tabId).SingleOrDefault(tv => !tv.IsPublished);
        }

        private static TabVersionDetail CreateNewTabVersionDetailObjectFromModule(int tabVersionId, ModuleInfo module, int moduleVersion, TabVersionDetailAction action)
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

        private static TabVersionDetail CreateNewTabVersionDetailObjectFromModule(int tabVersionId, int moduleId, string paneName, int moduleOrder, int moduleVersion, TabVersionDetailAction action)
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
        #endregion

        protected override Func<ITabVersionTracker> GetFactory()
        {
            return () => new TabVersionTracker();
        }
    }
}
