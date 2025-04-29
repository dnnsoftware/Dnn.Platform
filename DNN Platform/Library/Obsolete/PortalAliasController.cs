// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Entities.Portals;

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

public partial class PortalAliasController : ServiceLocator<IPortalAliasController, PortalAliasController>, IPortalAliasController
{
    [Obsolete("Deprecated in DotNetNuke 9.7.2. Use DotNetNuke.Abstractions.Portals.IPortalAliasService via dependency injection instead. Scheduled removal in v11.0.0.")]

    public static new IPortalAliasController Instance
    {
        get
        {
            var portalAliasSettingsService = Globals.DependencyProvider.GetRequiredService<IPortalAliasService>();
            return portalAliasSettingsService is IPortalAliasController castedController ? castedController : new PortalAliasController();
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Replaced by PortalAliasController.Instance.GetPortalAlias", RemovalVersion = 10)]
    public static partial PortalAliasInfo GetPortalAliasInfo(string httpAlias)
    {
        return Instance.GetPortalAlias(httpAlias);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 1, 0, "Replaced by PortalAliasController.Instance.GetPortalAliases", RemovalVersion = 10)]
    public static partial PortalAliasCollection GetPortalAliasLookup()
    {
        var portalAliasCollection = new PortalAliasCollection();
        var aliasController = new PortalAliasController();
        foreach (var kvp in aliasController.GetPortalAliasesInternal())
        {
            portalAliasCollection.Add(kvp.Key, kvp.Value);
        }

        return portalAliasCollection;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Replaced by PortalAliasController.Instance.GetPortalAlias", RemovalVersion = 10)]
    public static partial PortalAliasInfo GetPortalAliasLookup(string httpAlias)
    {
        return Instance.GetPortalAlias(httpAlias);
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

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 1, 0, "Replaced by PortalAliasController.Instance.DeletePortalAlias", RemovalVersion = 10)]
    public partial void DeletePortalAlias(int portalAliasId)
    {
        DataProvider.Instance().DeletePortalAlias(portalAliasId);

        EventLogController.Instance.AddLog(
            "PortalAliasID",
            portalAliasId.ToString(),
            PortalController.Instance.GetCurrentPortalSettings(),
            UserController.Instance.GetCurrentUserInfo().UserID,
            EventLogController.EventLogType.PORTALALIAS_DELETED);

        DataCache.RemoveCache(DataCache.PortalAliasCacheKey);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 1, 0, "Replaced by PortalAliasController.Instance.GetPortalAliasesByPortalId", RemovalVersion = 10)]
    public partial ArrayList GetPortalAliasArrayByPortalID(int portalID)
    {
        return new ArrayList(Instance.GetPortalAliasesByPortalId(portalID).ToArray());
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 1, 0, "Replaced by PortalAliasController.Instance.GetPortalAliasesByPortalId", RemovalVersion = 10)]
    public partial PortalAliasCollection GetPortalAliasByPortalID(int portalID)
    {
        var portalAliasCollection = new PortalAliasCollection();

        foreach (PortalAliasInfo alias in GetPortalAliasLookup().Values.Cast<PortalAliasInfo>().Where(alias => alias.PortalID == portalID))
        {
            portalAliasCollection.Add(alias.HTTPAlias, alias);
        }

        return portalAliasCollection;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 1, 0, "Replaced by PortalAliasController.Instance.UpdatePortalAlias", RemovalVersion = 10)]
    public partial void UpdatePortalAliasInfo(PortalAliasInfo portalAlias)
    {
        Instance.UpdatePortalAlias(portalAlias);
    }

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
    public partial PortalAliasInfo GetPortalAliasByPortalAliasID(int portalAliasId) =>
        (PortalAliasInfo)((IPortalAliasService)this).GetPortalAliasByPortalAliasId(portalAliasId);

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
    public partial PortalInfo GetPortalByPortalAliasID(int portalAliasId) =>
        (PortalInfo)((IPortalAliasService)this).GetPortalByPortalAliasId(portalAliasId);

    /// <inheritdoc cref="IPortalAliasService.UpdatePortalAlias"/>
    [DnnDeprecated(9, 7, 2, "use DotNetNuke.Abstractions.Portals.IPortalAliasService via dependency injection instead")]
    public partial void UpdatePortalAlias(PortalAliasInfo portalAlias) =>
        ((IPortalAliasService)this).UpdatePortalAlias(portalAlias);
}
