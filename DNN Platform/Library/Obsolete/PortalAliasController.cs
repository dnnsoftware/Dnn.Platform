// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <content>The obsolete methods for <see cref="PortalAliasController"/>.</content>
    public partial class PortalAliasController : ServiceLocator<IPortalAliasController, PortalAliasController>, IPortalAliasController
    {
        [Obsolete("Deprecated in DotNetNuke 9.7.2. Use DotNetNuke.Abstractions.Portals.IPortalAliasService via dependency injection instead. Scheduled removal in v11.0.0.")]
        public static new IPortalAliasController Instance
        {
            get
            {
                var portalAliasSettingsService = Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>();
                return portalAliasSettingsService is IPortalAliasController castedController ? castedController : new PortalAliasController();
            }
        }

        /// <summary>Gets the portal alias by portal.</summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <returns>Portal alias.</returns>
        [DnnDeprecated(9, 7, 2, "use DotNetNuke.Abstractions.Portals.IPortalAliasService via dependency injection instead")]
        public static partial string GetPortalAliasByPortal(int portalId, string portalAlias) =>
            ((IPortalAliasService)Instance).GetPortalAliasByPortal(portalId, portalAlias);

        /// <summary>Gets the portal alias by tab.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <returns>Portal alias.</returns>
        [DnnDeprecated(9, 7, 2, "use DotNetNuke.Abstractions.Portals.IPortalAliasService via dependency injection instead")]
        public static partial string GetPortalAliasByTab(int tabId, string portalAlias) =>
            ((IPortalAliasService)Instance).GetPortalAliasByTab(tabId, portalAlias);

        /// <summary>Validates the alias.</summary>
        /// <param name="portalAlias">The portal alias.</param>
        /// <param name="ischild">if set to <see langword="true"/> [ischild].</param>
        /// <returns><see langword="true"/> if the alias is a valid url format; otherwise return <see langword="false"/>.</returns>
        [DnnDeprecated(9, 7, 2, "use DotNetNuke.Abstractions.Portals.IPortalAliasService via dependency injection instead")]
        public static partial bool ValidateAlias(string portalAlias, bool ischild) =>
            ((IPortalAliasService)Instance).ValidateAlias(portalAlias, ischild);

        /// <inheritdoc cref="IPortalAliasService.AddPortalAlias"/>
        [DnnDeprecated(9, 7, 2, "use DotNetNuke.Abstractions.Portals.IPortalAliasService via dependency injection instead")]
        public partial int AddPortalAlias(PortalAliasInfo portalAlias) =>
            ((IPortalAliasService)this).AddPortalAlias(portalAlias);

        /// <inheritdoc cref="IPortalAliasService.DeletePortalAlias"/>
        [DnnDeprecated(9, 7, 2, "use DotNetNuke.Abstractions.Portals.IPortalAliasService via dependency injection instead")]
        public partial void DeletePortalAlias(PortalAliasInfo portalAlias) =>
            ((IPortalAliasService)this).DeletePortalAlias(portalAlias);

        /// <inheritdoc cref="IPortalAliasService.GetPortalAlias(string)"/>
        [DnnDeprecated(9, 7, 2, "use DotNetNuke.Abstractions.Portals.IPortalAliasService via dependency injection instead")]
        public partial PortalAliasInfo GetPortalAlias(string alias) =>
            (PortalAliasInfo)((IPortalAliasService)this).GetPortalAlias(alias);

        /// <summary>Gets the portal alias.</summary>
        /// <param name="alias">The portal alias.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>Portal Alias Info.</returns>
        [DnnDeprecated(9, 7, 2, "use DotNetNuke.Abstractions.Portals.IPortalAliasService via dependency injection instead")]
        public partial PortalAliasInfo GetPortalAlias(string alias, int portalId) =>
            (PortalAliasInfo)((IPortalAliasService)this).GetPortalAlias(alias, portalId);

        /// <summary>Gets the portal alias by portal alias ID.</summary>
        /// <param name="portalAliasId">The portal alias ID.</param>
        /// <returns>Portal alias info.</returns>
        [DnnDeprecated(9, 7, 2, "use DotNetNuke.Abstractions.Portals.IPortalAliasService via dependency injection instead")]
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public partial PortalAliasInfo GetPortalAliasByPortalAliasID(int portalAliasId) =>
            (PortalAliasInfo)((IPortalAliasService)this).GetPortalAliasByPortalAliasId(portalAliasId);
#pragma warning restore CS3005 // Identifier differing only in case is not CLS-compliant

        /// <inheritdoc cref="IPortalAliasService.GetPortalAliases"/>
        [DnnDeprecated(9, 7, 2, "use DotNetNuke.Abstractions.Portals.IPortalAliasService via dependency injection instead")]
        public partial PortalAliasCollection GetPortalAliases()
        {
            var aliasCollection = new PortalAliasCollection();
            foreach (var alias in this.GetPortalAliasesInternal().Values)
            {
                aliasCollection.Add(alias.HTTPAlias, alias);
            }

            return aliasCollection;
        }

        /// <inheritdoc cref="IPortalAliasService.GetPortalAliasByPortalAliasId"/>
        [DnnDeprecated(9, 7, 2, "use DotNetNuke.Abstractions.Portals.IPortalAliasService via dependency injection instead")]
        public partial IEnumerable<PortalAliasInfo> GetPortalAliasesByPortalId(int portalId) =>
            ((IPortalAliasService)this).GetPortalAliasesByPortalId(portalId)
                .Cast<PortalAliasInfo>();

        /// <summary>Gets the portal by portal alias ID.</summary>
        /// <param name="portalAliasId">The portal alias id.</param>
        /// <returns>Portal info.</returns>
        [DnnDeprecated(9, 7, 2, "use DotNetNuke.Abstractions.Portals.IPortalAliasService via dependency injection instead")]
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public partial PortalInfo GetPortalByPortalAliasID(int portalAliasId) =>
            (PortalInfo)((IPortalAliasService)this).GetPortalByPortalAliasId(portalAliasId);
#pragma warning restore CS3005 // Identifier differing only in case is not CLS-compliant

        /// <inheritdoc cref="IPortalAliasService.UpdatePortalAlias"/>
        [DnnDeprecated(9, 7, 2, "use DotNetNuke.Abstractions.Portals.IPortalAliasService via dependency injection instead")]
        public partial void UpdatePortalAlias(PortalAliasInfo portalAlias) =>
            ((IPortalAliasService)this).UpdatePortalAlias(portalAlias);
    }
}
