// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Modules.ExportImport
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Dnn.ExportImport.Dto.Assets;
    using Dnn.ExportImport.Repository;

    using NUnit.Framework;

    /// <summary>
    /// Tests for <see cref="ExportImportRepository"/> opening legacy (LiteDB v3.x, on-disk "v7")
    /// export databases produced by DNN 9.10.2-era instances.
    /// </summary>
    /// <remarks>
    /// Regression coverage for the site import failure where <c>VerifyImportPackage</c> returned HTTP 400
    /// <c>Package is not valid. Technical Details:Detected loop in FindAll({0})</c> when importing a legacy
    /// package whose largest collection exceeds the LiteDB 5.0.21 rebuild loop-guard threshold (~2,550 rows).
    /// The fixtures are genuine LiteDB v3.1.0 files (format version 7): <c>legacy_v3_large.dnndb</c> holds
    /// 3,000 <c>ExportFolder</c> rows (above the threshold — reproduces the original failure on the old
    /// <c>Upgrade = true</c> path) and <c>legacy_v3_small.dnndb</c> holds 50 rows (below the threshold —
    /// regression check that small legacy packages keep working).
    /// </remarks>
    [TestFixture]
    public class ExportImportRepositoryTests
    {
        private const int LargeFixtureFolderCount = 3000;
        private const int SmallFixtureFolderCount = 50;

        private string workingDirectory;

        [SetUp]
        public void SetUp()
        {
            this.workingDirectory = Path.Combine(Path.GetTempPath(), "DnnExportImportTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.workingDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (Directory.Exists(this.workingDirectory))
                {
                    Directory.Delete(this.workingDirectory, true);
                }
            }
            catch
            {
                // Best-effort cleanup of the per-test working directory.
            }
        }

        [Test]
        public void Constructor_WithLargeLegacyDatabase_OpensAndReturnsAllRows()
        {
            // Arrange
            var dbPath = this.CopyFixture("legacy_v3_large.dnndb");

            // Act & Assert - the old Upgrade=true path threw "Detected loop in FindAll({0})" here.
            using (var repository = new ExportImportRepository(dbPath))
            {
                Assert.That(repository.GetCount<ExportFolder>(), Is.EqualTo(LargeFixtureFolderCount));

                var all = repository.GetAllItems<ExportFolder>().ToList();
                Assert.That(all, Has.Count.EqualTo(LargeFixtureFolderCount));

                // Documents and their identity survive the legacy-to-5.x migration.
                var first = repository.GetItem<ExportFolder>(1);
                Assert.That(first, Is.Not.Null);
                Assert.That(first.FolderPath, Is.EqualTo("F/1"));

                var last = repository.GetItem<ExportFolder>(LargeFixtureFolderCount);
                Assert.That(last, Is.Not.Null);
                Assert.That(last.FolderPath, Is.EqualTo("F/" + LargeFixtureFolderCount));

                // Predicate queries (used throughout the import) work against the migrated database.
                var referenced = repository.GetItems<ExportFolder>(f => f.ReferenceId == 5).ToList();
                Assert.That(referenced, Is.Not.Empty);
                Assert.That(referenced, Has.All.Matches<ExportFolder>(f => f.ReferenceId == 5));
            }
        }

        [Test]
        public void Constructor_WithSmallLegacyDatabase_OpensWithoutRegression()
        {
            // Arrange
            var dbPath = this.CopyFixture("legacy_v3_small.dnndb");

            // Act & Assert - below the LiteDB rebuild threshold; must keep working.
            using (var repository = new ExportImportRepository(dbPath))
            {
                Assert.That(repository.GetCount<ExportFolder>(), Is.EqualTo(SmallFixtureFolderCount));
                Assert.That(repository.GetAllItems<ExportFolder>().ToList(), Has.Count.EqualTo(SmallFixtureFolderCount));
            }
        }

        [Test]
        public void Constructor_WithNativeFiveDatabase_OpensWithoutRegression()
        {
            // Arrange - a freshly created (native LiteDB 5.x) database, i.e. the common 10.x -> 10.x case.
            var dbPath = Path.Combine(this.workingDirectory, "native_v5.dnndb");
            using (var repository = new ExportImportRepository(dbPath))
            {
                for (var i = 1; i <= 100; i++)
                {
                    repository.CreateItem(new ExportFolder { FolderPath = "N/" + i }, null);
                }
            }

            // Act & Assert - reopening a native 5.x file must not trigger any migration and must round-trip.
            using (var repository = new ExportImportRepository(dbPath))
            {
                Assert.That(repository.GetCount<ExportFolder>(), Is.EqualTo(100));
            }
        }

        private string CopyFixture(string fixtureName)
        {
            var source = Path.Combine(TestDataDirectory(), fixtureName);
            Assert.That(File.Exists(source), Is.True, $"Missing test fixture: {source}");
            var destination = Path.Combine(this.workingDirectory, fixtureName);
            File.Copy(source, destination, true);
            return destination;
        }

        private static string TestDataDirectory()
        {
            var assemblyDir = Path.GetDirectoryName(new Uri(typeof(ExportImportRepositoryTests).Assembly.CodeBase).LocalPath);
            return Path.Combine(assemblyDir, "ExportImport", "TestData");
        }
    }
}
