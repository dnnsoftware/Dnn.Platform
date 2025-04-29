// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Internal;

using System;
using System.Collections.Generic;
using System.ComponentModel;

using DotNetNuke.Framework;
using DotNetNuke.Internal.SourceGenerators;

[EditorBrowsable(EditorBrowsableState.Never)]
[DnnDeprecated(7, 3, 0, "Please use PortalAliasController instead", RemovalVersion = 10)]
public partial class TestablePortalAliasController : ServiceLocator<IPortalAliasController, TestablePortalAliasController>, IPortalAliasController
{
    /// <inheritdoc/>
    public int AddPortalAlias(PortalAliasInfo portalAlias)
    {
        return PortalAliasController.Instance.AddPortalAlias(portalAlias);
    }

    /// <inheritdoc/>
    public void DeletePortalAlias(PortalAliasInfo portalAlias)
    {
        PortalAliasController.Instance.DeletePortalAlias(portalAlias);
    }

    /// <inheritdoc/>
    public PortalAliasInfo GetPortalAlias(string alias)
    {
        return PortalAliasController.Instance.GetPortalAlias(alias);
    }

    /// <inheritdoc/>
    public IEnumerable<PortalAliasInfo> GetPortalAliasesByPortalId(int portalId)
    {
        return PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId);
    }

    /// <inheritdoc/>
    public IDictionary<string, PortalAliasInfo> GetPortalAliases()
    {
        var aliasController = new PortalAliasController();
        return aliasController.GetPortalAliasesInternal();
    }

    /// <inheritdoc/>
    public void UpdatePortalAlias(PortalAliasInfo portalAlias)
    {
        PortalAliasController.Instance.UpdatePortalAlias(portalAlias);
    }

    /// <inheritdoc/>
    protected override Func<IPortalAliasController> GetFactory()
    {
        return () => new TestablePortalAliasController();
    }
}
