// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Services.Search.Internals
{
    using System.Data;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Search.Internals;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Lucene.Net.Analysis.Cz;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    public class LuceneControllerImplTests
    {
        [Test]
        public void GetCustomAnalyzer_WithTheProvidedAnalyzer_ReturnsTheAnalyzerCorrectly()
        {
            FakeServiceProvider serviceProvider = null;
            try
            {
                // Arrange
                const string HostSettingsTableName = "HostSettings";
                const string SettingNameColumnName = "SettingName";
                const string SettingValueColumnName = "SettingValue";
                const string SettingIsSecureColumnName = "SettingIsSecure";
                const string CustomAnalyzerCacheKeyName = "Search_CustomAnalyzer";
                const string CzechAnalyzerTypeName = "Lucene.Net.Analysis.Cz.CzechAnalyzer, Lucene.Net.Contrib.Analyzers";
                var mockData = MockComponentProvider.CreateDataProvider();
                var hostSettings = new DataTable(HostSettingsTableName);
                var nameCol = hostSettings.Columns.Add(SettingNameColumnName);
                hostSettings.Columns.Add(SettingValueColumnName);
                hostSettings.Columns.Add(SettingIsSecureColumnName);
                hostSettings.PrimaryKey = new[] { nameCol };
                hostSettings.Rows.Add(CustomAnalyzerCacheKeyName, CzechAnalyzerTypeName, true);
                mockData.Setup(c => c.GetHostSettings()).Returns(hostSettings.CreateDataReader());
                var mockedApplicationStatusInfo = new Mock<IApplicationStatusInfo>();
                mockedApplicationStatusInfo.Setup(s => s.Status).Returns(UpgradeStatus.Install);
                mockedApplicationStatusInfo.Setup(s => s.ApplicationMapPath).Returns(string.Empty);

                serviceProvider = FakeServiceProvider.Setup(
                    services =>
                    {
                        services.AddSingleton(mockData.Object);
                        services.AddSingleton(mockedApplicationStatusInfo.Object);
                    });

                MockComponentProvider.CreateDataCacheProvider();
                DataCache.ClearCache();
                var luceneController = new LuceneControllerImpl();

                // Act
                var analyzer = luceneController.GetCustomAnalyzer();

                // Assert
                Assert.That(analyzer, Is.InstanceOf<CzechAnalyzer>());
            }
            finally
            {
                serviceProvider?.Dispose();
            }
        }
    }
}
