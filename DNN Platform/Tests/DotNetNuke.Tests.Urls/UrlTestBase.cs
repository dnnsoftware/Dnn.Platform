﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Data;

using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Tests.Utilities;

using NUnit.Framework;

namespace DotNetNuke.Tests.Urls
{
    public class UrlTestBase : DnnWebTest
    {
        public UrlTestBase(int portalId) : base(portalId)
        {
        }

        protected virtual string DefaultAlias { get; private set; }

        protected virtual string TestType { get { return String.Empty; } }

        #region SetUp and TearDown

        public virtual void SetUp()
        {
            this.ExecuteScriptFile(String.Format("{0}\\{1}\\{2}", this.TestType, this.GetTestFolder(), "SetUp.sql"));
        }

        public virtual void TestFixtureSetUp()
        {
            this.ExecuteScriptFile(String.Format("{0}\\{1}", this.TestType, "SetUp.sql"));
        }

        public virtual void TearDown()
        {
            this.ExecuteScriptFile(String.Format("{0}\\{1}\\{2}", this.TestType, this.GetTestFolder(), "TearDown.sql"));
        }

        public virtual void TestFixtureTearDown()
        {
            this.ExecuteScriptFile(String.Format("{0}\\{1}", this.TestType, "TearDown.sql"));
        }

        #endregion

        protected void CreateTab(string tabName)
        {
            var tab = new TabInfo { PortalID = this.PortalId, TabName = tabName };

            TabController.Instance.AddTab(tab);
        }

        private void ExecuteScriptFile(string fileName)
        {
            var sql = TestUtil.ReadStream(fileName);

            if (!String.IsNullOrEmpty(sql))
            {
                DataProvider.Instance().ExecuteScript(sql);
            }
        }

        private string GetTestFolder()
        {
            var testName = TestContext.CurrentContext.Test.Name;
            return testName.Substring(0, testName.IndexOf("_", StringComparison.Ordinal));
        }

        protected void GetDefaultAlias()
        {
            foreach (var alias in PortalAliasController.Instance.GetPortalAliasesByPortalId(this.PortalId))
            {
                if (alias.IsPrimary)
                {
                    this.DefaultAlias = alias.HTTPAlias;
                    break;
                }
            }
        }

        protected void SetDefaultAlias(Dictionary<string, string> testFields)
        {
            this.SetDefaultAlias(testFields["Alias"]);
        }

        protected void SetDefaultAlias(string defaultAlias)
        {
            foreach (var alias in PortalAliasController.Instance.GetPortalAliasesByPortalId(this.PortalId))
            {
                if (string.Equals(alias.HTTPAlias, defaultAlias, StringComparison.InvariantCultureIgnoreCase))
                {
                    alias.IsPrimary = true;
                    PortalAliasController.Instance.UpdatePortalAlias(alias);
                    break;
                }
            }
        }



    }
}
