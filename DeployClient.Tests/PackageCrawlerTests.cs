using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DeployClient.Tests
{
    [TestClass]
    public class PackageCrawlerTests
    {
        private string _currentExecutionPath;


        [TestInitialize]
        public void Initialize()
        {
            _currentExecutionPath = $@"{Environment.CurrentDirectory}\DeployClient";
        }


        #region constructor tests

        [TestMethod]
        public void FileCrawler_Should_Use_Execution_Directory_When_Null_Argument()
        {
            // Arrange
            var mockFileSystem = GetBasicPreparedMockFileSystem(_currentExecutionPath);

            // Act
            var fileCrawler = new PackageCrawler(mockFileSystem, null);

            // Assert
            fileCrawler.PackageDirectoryPath.Should().Be(_currentExecutionPath);
        }

        [TestMethod]
        public void FileCrawler_Should_Use_Execution_Directory_When_Empty_String_Argument()
        {
            // Arrange
            var mockFileSystem = GetBasicPreparedMockFileSystem(_currentExecutionPath);

            // Act
            var fileCrawler = new PackageCrawler(mockFileSystem, string.Empty);

            // Assert
            fileCrawler.PackageDirectoryPath.Should().Be(_currentExecutionPath);
        }

        [TestMethod]
        public void FileCrawler_Should_Use_Execution_Directory_When_Whitespaces_Argument()
        {
            // Arrange
            var mockFileSystem = GetBasicPreparedMockFileSystem(_currentExecutionPath);

            // Act
            var fileCrawler = new PackageCrawler(mockFileSystem, "   ");

            // Assert
            fileCrawler.PackageDirectoryPath.Should().Be(_currentExecutionPath);
        }
        
        [TestMethod]
        public void FileCrawler_Should_Throw_Exception_When_Directory_Not_Exist()
        {
            // Arrange
            var mockFileSystem = GetBasicPreparedMockFileSystem(_currentExecutionPath);

            // Act
            // ReSharper disable once ObjectCreationAsStatement
            Action initialization = () => new PackageCrawler(mockFileSystem, "FooBar");

            // Assert
            initialization.Should().Throw<DirectoryNotFoundException>();
        }
        
        [TestMethod]
        public void FileCrawler_Should_Initialize_With_Provided_Directory_When_Directory_Exist()
        {
            // Arrange
            const string directoryName = "Packages";

            var mockFileSystem = GetBasicPreparedMockFileSystem(_currentExecutionPath);
            mockFileSystem.Directory.CreateDirectory(directoryName);

            // Act
            var fileCrawler = new PackageCrawler(mockFileSystem, directoryName);

            // Assert
            fileCrawler.PackageDirectoryPath.Should().Be($@"{_currentExecutionPath}\{directoryName}");
        }



        [TestMethod]
        public void FileCrawler_Should_Initialize_With_Provided_Directory_When_Directory_Exist_And_Full_Path()
        {
            // Arrange
            const string directoryName = "Packages";

            var mockFileSystem = GetBasicPreparedMockFileSystem(_currentExecutionPath);
            var directory = mockFileSystem.Directory.CreateDirectory(directoryName);

            var packageDirectoryPath = directory.FullName;

            // Act
            var fileCrawler = new PackageCrawler(mockFileSystem, packageDirectoryPath);

            // Assert
            fileCrawler.PackageDirectoryPath.Should().Be(packageDirectoryPath);
        }

        #endregion

        #region GetPackagesFullPaths tests

        [TestMethod]
        public void GetPackagesFullPaths_Should_Return_Empty_Enumerable_When_Directory_Empty()
        {
            // Arrange
            const string directoryName = "Packages";

            var mockFileSystem = GetBasicPreparedMockFileSystem(_currentExecutionPath);
            mockFileSystem.Directory.CreateDirectory(directoryName);
            
            var packageCrawler = new PackageCrawler(mockFileSystem, directoryName);

            // Act
            var packages = packageCrawler.GetPackagesFullPaths();

            // Assert
            packages.Should().BeEmpty();
        }

        [TestMethod]
        public void GetPackagesFullPaths_Should_Return_Zip_Files_Full_Paths_When_Directory_Contains_Zip_Files_Only()
        {
            // Arrange
            const string directoryName = "Packages";

            var mockFiles = GetBasicMockFiles();
            mockFiles.Add($@"{_currentExecutionPath}\Packages\TestPackage_1.zip", new MockFileData("#1 fake zip file."));
            mockFiles.Add($@"{_currentExecutionPath}\Packages\TestPackage_2.zip", new MockFileData("#2 fake zip file."));
            mockFiles.Add($@"{_currentExecutionPath}\Packages\TestPackage_3.zip", new MockFileData("#3 fake zip file."));

            var mockFileSystem = new MockFileSystem(mockFiles, _currentExecutionPath);

            var packageCrawler = new PackageCrawler(mockFileSystem, directoryName);

            // Act
            var packages = packageCrawler.GetPackagesFullPaths();

            // Assert
            packages.Should().HaveCount(3);
        }

        [TestMethod]
        public void GetPackagesFullPaths_Should_Return_Only_Zip_Files_Full_Paths_When_Directory_Contains_Different_File_Kinds()
        {
            // Arrange
            const string directoryName = "Packages";

            var mockFiles = GetBasicMockFiles();
            mockFiles.Add($@"{_currentExecutionPath}\Packages\TestPackage.zip", new MockFileData("A fake zip file."));
            mockFiles.Add($@"{_currentExecutionPath}\Packages\TestTextFile.txt", new MockFileData("Just a text file."));

            var mockFileSystem = new MockFileSystem(mockFiles, _currentExecutionPath);

            var packageCrawler = new PackageCrawler(mockFileSystem, directoryName);

            // Act
            var packages = packageCrawler.GetPackagesFullPaths();

            // Assert
            packages.Should().HaveCount(1);
        }

        [TestMethod]
        public void GetPackagesFullPaths_Should_Return_Zip_Files_Full_Paths_When_Directory_Contains_Different_File_Kinds_And_Full_Path_Initialization()
        {
            // Arrange
            var packageDirectoryPath = $@"{_currentExecutionPath}\Packages";

            var mockFiles = GetBasicMockFiles();
            mockFiles.Add($@"{packageDirectoryPath}\TestPackage.zip", new MockFileData("A fake zip file."));
            mockFiles.Add($@"{packageDirectoryPath}\TestTextFile.txt", new MockFileData("Just a text file."));

            var mockFileSystem = new MockFileSystem(mockFiles, _currentExecutionPath);

            var packageCrawler = new PackageCrawler(mockFileSystem, packageDirectoryPath);

            // Act
            var packages = packageCrawler.GetPackagesFullPaths();

            // Assert
            packages.Should().HaveCount(1);
        }

        [TestMethod]
        public void GetPackagesFullPaths_Should_Return_Zip_Files_Of_Top_Directory_Only_When_Sub_Directory_Exists_With_Zip_Files()
        {
            // Arrange
            const string directoryName = "Packages";

            var mockFiles = GetBasicMockFiles();
            mockFiles.Add($@"{_currentExecutionPath}\Packages\TestPackage.zip", new MockFileData("A fake zip file."));
            mockFiles.Add($@"{_currentExecutionPath}\Packages\SubDirectory\SubDir_TestPackage.zip", new MockFileData("A fake zip file from sub-directory."));

            var mockFileSystem = new MockFileSystem(mockFiles, _currentExecutionPath);

            var packageCrawler = new PackageCrawler(mockFileSystem, directoryName);

            // Act
            var packages = packageCrawler.GetPackagesFullPaths();

            // Assert
            packages.Should().HaveCount(1);
        }

        #endregion


        #region helper methods

        private IFileSystem GetBasicPreparedMockFileSystem(string currentDirectoryPath)
        {
            return new MockFileSystem(GetBasicMockFiles(), currentDirectoryPath);
        }

        private IDictionary<string, MockFileData> GetBasicMockFiles()
        {
            return new Dictionary<string, MockFileData>
            {
                {
                    $@"{_currentExecutionPath}\DeployClient_dummy.txt",
                    new MockFileData("Represents the executable to know where we are.")
                }
            };
        }

        #endregion
    }
}
