﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;

using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Portals.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class has been obsoleted in 7.3.0 - please use PortalAliasController instead. Scheduled removal in v10.0.0.")]
    public class TestablePortalAliasController : ServiceLocator<IPortalAliasController, TestablePortalAliasController>, IPortalAliasController
    {
        protected override Func<IPortalAliasController> GetFactory()
        {
            return () => new TestablePortalAliasController();
        }

        public int AddPortalAlias(PortalAliasInfo portalAlias)
        {
            return PortalAliasController.Instance.AddPortalAlias(portalAlias);
        }

        public void DeletePortalAlias(PortalAliasInfo portalAlias)
        {
            PortalAliasController.Instance.DeletePortalAlias(portalAlias);
        }

        public PortalAliasInfo GetPortalAlias(string alias)
        {
            return PortalAliasController.Instance.GetPortalAlias(alias);
        }

        public IEnumerable<PortalAliasInfo> GetPortalAliasesByPortalId(int portalId)
        {
            return PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId);
        }

        public IDictionary<string, PortalAliasInfo> GetPortalAliases()
        {
            var aliasController = new PortalAliasController();
            return aliasController.GetPortalAliasesInternal();
        }

        public void UpdatePortalAlias(PortalAliasInfo portalAlias)
        {
            PortalAliasController.Instance.UpdatePortalAlias(portalAlias);
        }
    }
}
