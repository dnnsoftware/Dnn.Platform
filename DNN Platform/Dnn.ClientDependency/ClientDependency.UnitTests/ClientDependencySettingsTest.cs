using System.Configuration;
using System.Reflection;
using ClientDependency.Core;
using ClientDependency.Core.Config;
using NUnit.Framework;
using System;
using ClientDependency.Core.CompositeFiles.Providers;
using ClientDependency.Core.FileRegistration.Providers;
using System.Collections.Generic;
using ClientDependency.Core.Logging;
using System.IO;
using Rhino.Mocks;
using System.Web;

namespace ClientDependency.UnitTests
{

    

    /// <summary>
    ///This is a test class for ClientDependencySettingsTest and is intended
    ///to contain all ClientDependencySettingsTest Unit Tests
    ///</summary>
    [TestFixture]
    public class SettingsTest
    {
        
        private static ClientDependencySection GetSection(string fileName)
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            var binFolder = Path.GetDirectoryName(path);
            var configFile = new FileInfo(binFolder + "\\..\\..\\" + fileName);
            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = configFile.FullName };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            return (ClientDependencySection)configuration.GetSection("clientDependency");
        }

        [Test]
        public void Settings_Legacy_Pre_13()
        {
            //Arrange

            var ctxFactory = new FakeHttpContextFactory("~/somesite/hello");
            var configSection = GetSection("LegacyConfig1.3.Config");
            StringExtensions.GetConfigSection = () => configSection;
            NetworkHelper.GetConfigSection = () => configSection;

            //Act

            //set the xml file mapper folder to be the default for file processing provider at runtime
            XmlFileMapper.FileMapVirtualFolder = "~/App_Data/TEMP/ClientDependency";
            BaseCompositeFileProcessingProvider.UrlTypeDefault = CompositeUrlType.Base64QueryStrings;
            var settings = new ClientDependencySettings(configSection, ctxFactory.Context);            

            //Assert

            Assert.AreEqual(typeof(LoaderControlProvider), settings.DefaultFileRegistrationProvider.GetType());
            Assert.AreEqual(123456, settings.Version);
            foreach(var i in ".js,.css,.less".Split(','))
            {
                Assert.IsTrue(settings.FileBasedDependencyExtensionList.Contains(i.ToUpper()));                
            }
            Assert.AreEqual(typeof(StandardRenderer), settings.DefaultMvcRenderer.GetType());
            Assert.AreEqual(typeof(CompositeFileProcessingProvider), settings.DefaultCompositeFileProcessingProvider.GetType());
            Assert.AreEqual(1, settings.ConfigSection.CompositeFileElement.MimeTypeCompression.Count);
            Assert.AreEqual(0, settings.ConfigSection.CompositeFileElement.RogueFileCompression.Count);

            Assert.AreEqual(settings.DefaultCompositeFileProcessingProvider.CompositeFilePathAsString, "~/App_Data/TEMP/ClientDependency");
            Assert.AreEqual(settings.DefaultCompositeFileProcessingProvider.CompositeFilePathAsString, XmlFileMapper.FileMapVirtualFolder);
            Assert.AreEqual(CompositeUrlType.Base64QueryStrings, settings.DefaultCompositeFileProcessingProvider.UrlType);
        }

        /// <summary>
        ///A test for all sections defined
        ///</summary>
        [Test]
        public void Settings_All_Sections_Defined()
        {
            //Arrange

            var ctxFactory = new FakeHttpContextFactory("~/somesite/hello");
            var configSection = GetSection("AllSections.Config");
            StringExtensions.GetConfigSection = () => configSection;
            NetworkHelper.GetConfigSection = () => configSection;

            //Act

            var settings = new ClientDependencySettings(configSection, ctxFactory.Context);

            //Assert

            Assert.AreEqual(typeof(PageHeaderProvider), settings.DefaultFileRegistrationProvider.GetType());
            Assert.AreEqual(123456, settings.Version);
            Assert.AreEqual(typeof (LazyLoadRenderer), settings.DefaultMvcRenderer.GetType());
            Assert.AreEqual(typeof (CompositeFileProcessingProvider), settings.DefaultCompositeFileProcessingProvider.GetType());
            Assert.AreEqual(2, settings.ConfigSection.CompositeFileElement.MimeTypeCompression.Count);
            Assert.AreEqual(1, settings.ConfigSection.CompositeFileElement.RogueFileCompression.Count);
        }

        /// <summary>
        ///A test for CompositeFileProcessingProviderCollection
        ///</summary>
        [Test]
        public void Settings_Min_Sections_Defined()
        {
            //Arrange

            var ctxFactory = new FakeHttpContextFactory("~/somesite/hello");
            var configSection = GetSection("MinSections.Config");
            StringExtensions.GetConfigSection = () => configSection;
            NetworkHelper.GetConfigSection = () => configSection;

            //Act

            var settings = new ClientDependencySettings(configSection, ctxFactory.Context);

            //Assert

            Assert.AreEqual(typeof(PlaceHolderProvider), settings.DefaultFileRegistrationProvider.GetType());
            Assert.AreEqual(11111, settings.Version);
            Assert.AreEqual(typeof(StandardRenderer), settings.DefaultMvcRenderer.GetType());
            Assert.AreEqual(typeof(CompositeFileProcessingProvider), settings.DefaultCompositeFileProcessingProvider.GetType());
            Assert.AreEqual(0, settings.ConfigSection.CompositeFileElement.MimeTypeCompression.Count);
            Assert.AreEqual(0, settings.ConfigSection.CompositeFileElement.RogueFileCompression.Count);

            Assert.AreEqual("/DependencyHandler.axd", settings.DefaultFileRegistrationProvider.GetCompositeFileHandlerPath(ctxFactory.Context));
        }

        
    }
}
