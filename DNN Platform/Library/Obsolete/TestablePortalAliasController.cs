#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Collections.Generic;
using System.ComponentModel;

using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Portals.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class has been obsoleted in 7.3.0 - please use PortalAliasController instead")]
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