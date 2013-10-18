// <copyright file="ExtensionUrlProviderControllerTests.cs" company="Engage Software">
// DotNetNuke.Tests.Urls
// Copyright (c) 2004-2013
// by Engage Software ( http://www.engagesoftware.com )
// </copyright>
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
namespace DotNetNuke.Tests.Urls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;

    using NUnit.Framework;

    [TestFixture]
    public class ExtensionUrlProviderControllerTests
    {
        [Test]
        public void GetModuleProviders_ExcludeSingleProviderWithTypeThatDoesNotExist()
        {
            var getExtensionUrlProvidersDataSet = GetDataSetForExtensionUrlProvidersCall();
            var providersTable = getExtensionUrlProvidersDataSet.Tables[0];
            AddProviderRow(providersTable, providerType: "This.Is.Not.A.Real.Type");
            SetupGetModuleProvidersCall(getExtensionUrlProvidersDataSet.CreateDataReader());

            var providers = ExtensionUrlProviderController.GetModuleProviders(Constants.PORTAL_ValidPortalId);

            Assert.IsNotNull(providers, "Providers list should be empty, not null");
            Assert.IsEmpty(providers, "Providers list should be empty, since there is only an invalid entry");
        }

        [Test]
        public void GetModuleProviders_OnlyExcludeProviderWithTypeThatDoesNotExistButIncludeOther()
        {
            var getExtensionUrlProvidersDataSet = GetDataSetForExtensionUrlProvidersCall();
            var providersTable = getExtensionUrlProvidersDataSet.Tables[0];
            AddProviderRow(providersTable, providerName: "Broken Provider", providerType: "This.Is.Not.A.Real.Type");
            AddProviderRow(providersTable, providerName: "Working Provider", providerType: typeof(FakeExtensionUrlProvider).AssemblyQualifiedName);
            SetupGetModuleProvidersCall(getExtensionUrlProvidersDataSet.CreateDataReader());

            var providers = ExtensionUrlProviderController.GetModuleProviders(Constants.PORTAL_ValidPortalId);

            Assert.IsNotNull(providers, "Providers list should be empty, not null");
            Assert.AreEqual(1, providers.Count, "Providers list should have the one valid entry, but not the invalid entry");
            Assert.AreEqual("Working Provider", providers[0].ProviderConfig.ProviderName, "Providers list should have the valid entry");
        }

        private static void SetupGetModuleProvidersCall(IDataReader getExtensionUrlProvidersReader)
        {
            Globals.SetStatus(Globals.UpgradeStatus.None);
            MockComponentProvider.CreateDataCacheProvider();
            var dataProvider = MockComponentProvider.CreateDataProvider();

            dataProvider.Setup(dp => dp.GetExtensionUrlProviders(Constants.PORTAL_ValidPortalId)).Returns(getExtensionUrlProvidersReader);
        }

        private static DataSet GetDataSetForExtensionUrlProvidersCall()
        {
            var getExtensionUrlProvidersDataSet = new DataSet();

            var providersTable = getExtensionUrlProvidersDataSet.Tables.Add();
            providersTable.Columns.AddRange(
                new[]
                {
                    new DataColumn("ExtensionUrlProviderId", typeof(int)), new DataColumn("PortalId", typeof(int)),
                    new DataColumn("DesktopModuleId", typeof(int)), new DataColumn("ProviderName", typeof(string)),
                    new DataColumn("ProviderType", typeof(string)), new DataColumn("SettingsControlSrc", typeof(string)),
                    new DataColumn("IsActive", typeof(bool)), new DataColumn("RewriteAllUrls", typeof(bool)), new DataColumn("RedirectAllUrls", typeof(bool)),
                    new DataColumn("ReplaceAllUrls", typeof(bool)),
                });
            var providerSettingsTable = getExtensionUrlProvidersDataSet.Tables.Add();
            providerSettingsTable.Columns.AddRange(
                new[]
                {
                    new DataColumn("ExtensionUrlProviderID", typeof(int)), new DataColumn("PortalID", typeof(int)),
                    new DataColumn("SettingName", typeof(string)), new DataColumn("SettingValue", typeof(string)),
                });

            var providerTabsTable = getExtensionUrlProvidersDataSet.Tables.Add();
            providerTabsTable.Columns.AddRange(new[] { new DataColumn("ExtensionUrlProviderID", typeof(int)), new DataColumn("TabId", typeof(int)), });
            
            return getExtensionUrlProvidersDataSet;
        }

        private static void AddProviderRow(DataTable providersTable, int extensionUrlProviderId = 1, int portalId = Constants.PORTAL_ValidPortalId, int? desktopModuleId = null, string providerName = "", string providerType = "", string settingsControlSrc = "", bool isActive = true, bool rewriteAllUrls = true, bool redirectAllUrls = true, bool replaceAllUrls = true)
        {
            providersTable.Rows.Add(
                extensionUrlProviderId,
                portalId,
                desktopModuleId.HasValue ? desktopModuleId : (object)DBNull.Value,
                providerName,
                providerType,
                settingsControlSrc,
                isActive,
                rewriteAllUrls,
                redirectAllUrls,
                replaceAllUrls);
        }

        internal class FakeExtensionUrlProvider : ExtensionUrlProvider
        {
            public override bool AlwaysUsesDnnPagePath(int portalId)
            {
                throw new NotImplementedException();
            }

            public override string ChangeFriendlyUrl(TabInfo tab, string friendlyUrlPath, FriendlyUrlOptions options, string cultureCode, ref string endingPageName, out bool useDnnPagePath, ref List<string> messages) 
            {
                throw new NotImplementedException();
            }

            public override bool CheckForRedirect(int tabId, int portalid, string httpAlias, Uri requestUri, NameValueCollection queryStringCol, FriendlyUrlOptions options, out string redirectLocation, ref List<string> messages)
            {
                throw new NotImplementedException();
            }

            public override Dictionary<string, string> GetProviderPortalSettings()
            {
                throw new NotImplementedException();
            }

            public override string TransformFriendlyUrlToQueryString(string[] urlParms, int tabId, int portalId, FriendlyUrlOptions options, string cultureCode, PortalAliasInfo portalAlias, ref List<string> messages, out int status, out string location)
            {
                throw new NotImplementedException();
            }
        }
    }
}