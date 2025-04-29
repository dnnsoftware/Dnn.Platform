namespace DotNetNuke.Tests.Core.Services.Search.Internals;

using System.Data;

using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Search.Internals;
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
        // Arrange
        const string HostSettingsTableName = "HostSettings";
        const string SettingNameColumnName = "SettingName";
        const string SettingValueColumnName = "SettingValue";
        const string SettingIsSecureColumnName = "SettingIsSecure";
        const string CustomAnalyzerCacheKeyName = "Search_CustomAnalyzer";
        const string CzechAnalyzerTypeName = "Lucene.Net.Analysis.Cz.CzechAnalyzer, Lucene.Net.Contrib.Analyzers";
        var services = new ServiceCollection();
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
        services.AddTransient(container => Mock.Of<IHostSettingsService>());
        services.AddTransient(container => mockedApplicationStatusInfo.Object);
        services.AddTransient(container => Mock.Of<INavigationManager>());
        Globals.DependencyProvider = services.BuildServiceProvider();
        MockComponentProvider.CreateDataCacheProvider();
        DataCache.ClearCache();
        var luceneController = new LuceneControllerImpl();

        // Act
        var analyzer = luceneController.GetCustomAnalyzer();

        // Assert
        Assert.That(analyzer, Is.InstanceOf<CzechAnalyzer>());
    }
}
