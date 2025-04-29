// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.Localisation;

using System;
using System.Reflection;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.UI.WebControls;

/// <summary>Deprecated Apollo localization.</summary>
[DnnDeprecated(9, 4, 0, "No replacement", RemovalVersion = 10)]
public partial class Apollo : ILocalisation
{
    private bool haveChecked;
    private MethodInfo apiMember;

    /// <inheritdoc cref="ILocalisation.HaveApi"/>
    [DnnDeprecated(9, 4, 0, "No replacement", RemovalVersion = 10)]
    public partial bool HaveApi()
    {
        if (!this.haveChecked)
        {
            try
            {
                if (DesktopModuleController.GetDesktopModuleByModuleName("PageLocalization", PortalSettings.Current.PortalId) != null)
                {
                    var api = Activator.CreateInstance("Apollo.LocalizationApi", "Apollo.DNN_Localization.LocalizeTab").Unwrap();
                    var apiType = api.GetType();
                    this.apiMember = apiType.GetMethod("getLocalizedTab", new[] { typeof(TabInfo) });
                }
            }
            catch
            {
            }

            this.haveChecked = true;
        }

        return this.apiMember != null;
    }

    /// <inheritdoc cref="ILocalisation.LocaliseTab"/>
    [DnnDeprecated(9, 4, 0, "No replacement", RemovalVersion = 10)]
    public partial TabInfo LocaliseTab(TabInfo tab, int portalId)
    {
        return this.apiMember.Invoke(null, new object[] { tab }) as TabInfo ?? tab;
    }

    /// <inheritdoc cref="ILocalisation.LocaliseNodes"/>
    [DnnDeprecated(9, 4, 0, "No replacement", RemovalVersion = 10)]
    public partial DNNNodeCollection LocaliseNodes(DNNNodeCollection nodes)
    {
        return null;
    }
}
