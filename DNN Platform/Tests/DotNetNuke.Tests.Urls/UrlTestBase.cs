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
            ExecuteScriptFile(String.Format("{0}\\{1}\\{2}", TestType, GetTestFolder(), "SetUp.sql"));
        }

        public virtual void TestFixtureSetUp()
        {
            ExecuteScriptFile(String.Format("{0}\\{1}", TestType, "SetUp.sql"));
        }

        public virtual void TearDown()
        {
            ExecuteScriptFile(String.Format("{0}\\{1}\\{2}", TestType, GetTestFolder(), "TearDown.sql"));
        }

        public virtual void TestFixtureTearDown()
        {
            ExecuteScriptFile(String.Format("{0}\\{1}", TestType, "TearDown.sql"));
        }

        #endregion

        protected void CreateTab(string tabName)
        {
            var tab = new TabInfo { PortalID = PortalId, TabName = tabName };

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
            foreach (var alias in PortalAliasController.Instance.GetPortalAliasesByPortalId(PortalId))
            {
                if (alias.IsPrimary)
                {
                    DefaultAlias = alias.HTTPAlias;
                    break;
                }
            }
        }

        protected void SetDefaultAlias(Dictionary<string, string> testFields)
        {
            SetDefaultAlias(testFields["Alias"]);
        }

        protected void SetDefaultAlias(string defaultAlias)
        {
            foreach (var alias in PortalAliasController.Instance.GetPortalAliasesByPortalId(PortalId))
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
