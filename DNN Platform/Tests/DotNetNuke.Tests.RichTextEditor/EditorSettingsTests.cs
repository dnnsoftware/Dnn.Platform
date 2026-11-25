// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.RichTextEditor
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    using Dnn.RichTextEditorProvider.Components.Settings;
    using NUnit.Framework;

    [TestFixture]
    public class EditorSettingsTests
    {
        private const int PortalId = 3;

        [Test]
        public void ResolveToolbar_UsesLargestPresetOfTheUsersRoles()
        {
            var settings = new EditorSettings
            {
                ToolbarRoles = new Dictionary<string, string> { { "Registered Users", "basic" }, { "Editors", "standard" }, { "Administrators", "full" }, },
            };

            Assert.That(settings.ResolveToolbar(InRoles("Registered Users", "Editors"), false), Is.EqualTo("standard"));
        }

        [Test]
        public void ResolveToolbar_UsesDefault_WhenNoRoleMatches()
        {
            var settings = new EditorSettings
            {
                DefaultToolbar = "standard",
                ToolbarRoles = new Dictionary<string, string> { { "Administrators", "full" }, },
            };

            Assert.That(settings.ResolveToolbar(InRoles("Registered Users"), true), Is.EqualTo("standard"));
        }

        [Test]
        public void ResolveToolbar_GivesAdministratorsFullToolbar_WhenNoRolesAreConfigured()
        {
            var settings = new EditorSettings();

            Assert.Multiple(() =>
            {
                Assert.That(settings.ResolveToolbar(InRoles(), true), Is.EqualTo("full"));
                Assert.That(settings.ResolveToolbar(InRoles(), false), Is.EqualTo("basic"));
            });
        }

        [Test]
        public void ResolveUploadLimitBytes_UsesLargestLimitOfTheUsersRoles()
        {
            var settings = new EditorSettings
            {
                DefaultUploadLimitKb = 100,
                UploadLimitRoles = new Dictionary<string, int> { { "Registered Users", 500 }, { "Editors", 2048 }, },
            };

            Assert.Multiple(() =>
            {
                Assert.That(settings.ResolveUploadLimitBytes(InRoles("Registered Users", "Editors")), Is.EqualTo(2048L * 1024));
                Assert.That(settings.ResolveUploadLimitBytes(InRoles("Subscribers")), Is.EqualTo(100L * 1024));
                Assert.That(new EditorSettings().ResolveUploadLimitBytes(InRoles()), Is.Null);
            });
        }

        [Test]
        public void GetEffective_PrefersSiteThenAllSitesThenWebConfig()
        {
            var store = new FakeStore();
            store.Set(EditorSettingsRepository.AllSites, EditorSettingsRepository.DefaultToolbarKey, "standard");
            store.Set(EditorSettingsRepository.AllSites, EditorSettingsRepository.UploadFolderKey, "Shared/");
            store.Set(EditorSettingsRepository.AllSites, EditorSettingsRepository.ToolbarRolesKey, "{\"Administrators\":\"full\"}");
            store.Set(PortalId, EditorSettingsRepository.UploadFolderKey, "Images/");
            var attributes = new NameValueCollection { { "toolbar", "basic" }, { "uploadFolder", "FromConfig/" }, };

            var site = new EditorSettingsRepository(store).GetEffective(PortalId, attributes);
            var otherSite = new EditorSettingsRepository(new FakeStore()).GetEffective(PortalId, attributes);

            Assert.Multiple(() =>
            {
                Assert.That(site.UploadFolder, Is.EqualTo("Images/"));
                Assert.That(site.DefaultToolbar, Is.EqualTo("standard"));
                Assert.That(site.ToolbarRoles["administrators"], Is.EqualTo("full"));
                Assert.That(otherSite.UploadFolder, Is.EqualTo("FromConfig/"));
                Assert.That(otherSite.DefaultToolbar, Is.EqualTo("basic"));
            });
        }

        [Test]
        public void Load_ReadsMigratedCkEditorSettings()
        {
            // As written by Install/10.04.00.SqlDataProvider, including an escaped role name and a custom CKEditor toolbar name.
            var store = new FakeStore();
            store.Set(PortalId, EditorSettingsRepository.ToolbarRolesKey, "{\"Content \\\"Editors\\\"\":\"full\",\"Unauthenticated Users\":\"basic\",\"Other\":\"MyToolbar\"}");
            store.Set(PortalId, EditorSettingsRepository.UploadLimitRolesKey, "{\"Content \\\"Editors\\\"\":2,\"Bad\":0}");

            var settings = new EditorSettingsRepository(store).Load(PortalId);

            Assert.Multiple(() =>
            {
                Assert.That(settings.ToolbarRoles.Keys, Is.EquivalentTo(new[] { "Content \"Editors\"", "Unauthenticated Users", }));
                Assert.That(settings.ToolbarRoles["Content \"Editors\""], Is.EqualTo("full"));
                Assert.That(settings.UploadLimitRoles.Keys, Is.EqualTo(new[] { "Content \"Editors\"", }));
            });
        }

        [Test]
        public void Load_IgnoresDamagedValues()
        {
            var store = new FakeStore();
            store.Set(PortalId, EditorSettingsRepository.ToolbarRolesKey, "{not json");
            store.Set(PortalId, EditorSettingsRepository.DefaultUploadLimitKey, "-5");
            store.Set(PortalId, EditorSettingsRepository.DefaultToolbarKey, "huge");

            var settings = new EditorSettingsRepository(store).Load(PortalId);

            Assert.Multiple(() =>
            {
                Assert.That(settings.ToolbarRoles, Is.Null);
                Assert.That(settings.DefaultUploadLimitKb, Is.Null);
                Assert.That(settings.DefaultToolbar, Is.Null);
            });
        }

        [Test]
        public void Save_RoundTripsAndClearsUnsetValues()
        {
            var store = new FakeStore();
            var repository = new EditorSettingsRepository(store);
            repository.Save(PortalId, new EditorSettings
            {
                DefaultToolbar = "Full",
                ToolbarRoles = new Dictionary<string, string> { { "Editors", "standard" }, },
                UploadFolder = " Images/ ",
                DefaultUploadLimitKb = 1024,
            });

            var saved = repository.Load(PortalId);
            repository.Save(PortalId, new EditorSettings());
            var cleared = repository.Load(PortalId);

            Assert.Multiple(() =>
            {
                Assert.That(saved.DefaultToolbar, Is.EqualTo("full"));
                Assert.That(saved.ToolbarRoles["Editors"], Is.EqualTo("standard"));
                Assert.That(saved.UploadFolder, Is.EqualTo("Images/"));
                Assert.That(saved.DefaultUploadLimitKb, Is.EqualTo(1024));
                Assert.That(saved.UploadLimitRoles, Is.Null);
                Assert.That(cleared.DefaultToolbar, Is.Null);
                Assert.That(cleared.ToolbarRoles, Is.Null);
                Assert.That(cleared.UploadFolder, Is.Null);
                Assert.That(cleared.DefaultUploadLimitKb, Is.Null);
            });
        }

        private static Func<string, bool> InRoles(params string[] roles)
        {
            return role => roles.Contains(role);
        }

        private sealed class FakeStore : ISettingsStore
        {
            private readonly Dictionary<string, string> values = new Dictionary<string, string>();

            public string Get(int portalId, string key)
            {
                return this.values.TryGetValue(portalId + "|" + key, out var value) ? value : null;
            }

            public void Set(int portalId, string key, string value)
            {
                this.values[portalId + "|" + key] = value ?? string.Empty;
            }
        }
    }
}
