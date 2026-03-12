// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.DDRMenu.Localisation
{
    using System;
    using System.Linq;
    using System.Reflection;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.UI.WebControls;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Implements generic localization support.</summary>
    /// <param name="businessControllerProvider">The business controller provider.</param>
    /// <param name="hostSettings">The host settings.</param>
    public class Generic(IBusinessControllerProvider businessControllerProvider, IHostSettings hostSettings)
        : ILocalisation
    {
        private readonly IBusinessControllerProvider businessControllerProvider = businessControllerProvider ?? Globals.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.DependencyProvider.GetRequiredService<IHostSettings>();
        private bool haveChecked;
        private object locApi;
        private MethodInfo locTab;
        private MethodInfo locNodes;

        /// <summary>Initializes a new instance of the <see cref="Generic"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public Generic()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Generic"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public Generic(IBusinessControllerProvider businessControllerProvider)
            : this(businessControllerProvider, null)
        {
        }

        /// <inheritdoc />
        public bool HaveApi()
        {
            if (!this.haveChecked)
            {
                var modules = DesktopModuleController.GetDesktopModules(this.hostSettings, PortalSettings.Current.PortalId);
                foreach (var module in modules.Values.Where(m => !string.IsNullOrEmpty(m.BusinessControllerClass)))
                {
                    try
                    {
                        var controllerType = Reflection.CreateType(module.BusinessControllerClass);
                        this.locTab = controllerType.GetMethod("LocaliseTab", [typeof(TabInfo), typeof(int),]);
                        if (this.locTab != null)
                        {
                            if (!this.locTab.IsStatic)
                            {
                                this.locApi = this.businessControllerProvider.GetInstance(controllerType);
                            }

                            break;
                        }

                        this.locNodes = controllerType.GetMethod("LocaliseNodes", [typeof(DNNNodeCollection),]);
                        if (this.locNodes != null)
                        {
                            if (!this.locNodes.IsStatic)
                            {
                                this.locApi = this.businessControllerProvider.GetInstance(controllerType);
                            }

                            break;
                        }
                    }

                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                    }
                }

                this.haveChecked = true;
            }

            return this.locTab != null || this.locNodes != null;
        }

        /// <inheritdoc />
        public TabInfo LocaliseTab(TabInfo tab, int portalId)
        {
            return (TabInfo)this.locTab?.Invoke(this.locApi, [tab, portalId,]);
        }

        /// <inheritdoc />
        public DNNNodeCollection LocaliseNodes(DNNNodeCollection nodes)
        {
            return (DNNNodeCollection)this.locNodes?.Invoke(this.locApi, [nodes,]);
        }
    }
}
