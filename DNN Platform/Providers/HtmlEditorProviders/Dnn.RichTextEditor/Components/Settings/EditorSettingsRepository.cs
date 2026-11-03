// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Components.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Web.Script.Serialization;

    /// <summary>
    /// Loads and saves the editor's settings. Each setting is looked up for the site first, then for all sites,
    /// then in the provider's web.config attributes.
    /// </summary>
    public class EditorSettingsRepository
    {
        /// <summary>The scope of settings that apply to all sites unless a site overrides them.</summary>
        public const int AllSites = -1;

        /// <summary>The setting name of <see cref="EditorSettings.DefaultToolbar"/>. Also used by the migration script.</summary>
        public const string DefaultToolbarKey = "RichTextEditor_DefaultToolbar";

        /// <summary>The setting name of <see cref="EditorSettings.ToolbarRoles"/>, stored as a JSON object.</summary>
        public const string ToolbarRolesKey = "RichTextEditor_ToolbarRoles";

        /// <summary>The setting name of <see cref="EditorSettings.UploadFolder"/>.</summary>
        public const string UploadFolderKey = "RichTextEditor_UploadFolder";

        /// <summary>The setting name of <see cref="EditorSettings.DefaultUploadLimitKb"/>.</summary>
        public const string DefaultUploadLimitKey = "RichTextEditor_DefaultUploadLimitKb";

        /// <summary>The setting name of <see cref="EditorSettings.UploadLimitRoles"/>, stored as a JSON object.</summary>
        public const string UploadLimitRolesKey = "RichTextEditor_UploadLimitRoles";

        private readonly ISettingsStore store;

        /// <summary>Initializes a new instance of the <see cref="EditorSettingsRepository"/> class.</summary>
        /// <param name="store">The settings store.</param>
        public EditorSettingsRepository(ISettingsStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        /// <summary>Gets the settings stored at one scope, without falling back to other scopes.</summary>
        /// <param name="portalId">The site's portal ID, or <see cref="AllSites"/>.</param>
        /// <returns>The settings; unset values are <see langword="null"/>.</returns>
        public EditorSettings Load(int portalId)
        {
            return new EditorSettings
            {
                DefaultToolbar = ToolbarPresets.Normalize(this.store.Get(portalId, DefaultToolbarKey)),
                ToolbarRoles = ToRoleMap(
                    ReadMap<string>(this.store.Get(portalId, ToolbarRolesKey))?.Select(role => new KeyValuePair<string, string>(role.Key, ToolbarPresets.Normalize(role.Value))),
                    preset => preset != null),
                UploadFolder = NullIfEmpty(this.store.Get(portalId, UploadFolderKey)),
                DefaultUploadLimitKb = ReadLimit(this.store.Get(portalId, DefaultUploadLimitKey)),
                UploadLimitRoles = ToRoleMap(ReadMap<int>(this.store.Get(portalId, UploadLimitRolesKey)), limit => limit > 0),
            };
        }

        /// <summary>Gets the settings in effect for a site.</summary>
        /// <param name="portalId">The site's portal ID.</param>
        /// <param name="providerAttributes">The provider's web.config attributes, used when neither the site nor all sites set a value.</param>
        /// <returns>The effective settings.</returns>
        public EditorSettings GetEffective(int portalId, NameValueCollection providerAttributes)
        {
            var site = portalId == AllSites ? new EditorSettings() : this.Load(portalId);
            var allSites = this.Load(AllSites);
            return new EditorSettings
            {
                DefaultToolbar = site.DefaultToolbar ?? allSites.DefaultToolbar ?? ToolbarPresets.Normalize(providerAttributes?["toolbar"]),
                ToolbarRoles = site.ToolbarRoles ?? allSites.ToolbarRoles,
                UploadFolder = site.UploadFolder ?? allSites.UploadFolder ?? NullIfEmpty(providerAttributes?["uploadFolder"]),
                DefaultUploadLimitKb = site.DefaultUploadLimitKb ?? allSites.DefaultUploadLimitKb,
                UploadLimitRoles = site.UploadLimitRoles ?? allSites.UploadLimitRoles,
            };
        }

        /// <summary>Saves the settings for one scope. <see langword="null"/> values are cleared, so the scope falls back to the next one.</summary>
        /// <param name="portalId">The site's portal ID, or <see cref="AllSites"/>.</param>
        /// <param name="settings">The settings.</param>
        public void Save(int portalId, EditorSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this.store.Set(portalId, DefaultToolbarKey, ToolbarPresets.Normalize(settings.DefaultToolbar));
            this.store.Set(portalId, ToolbarRolesKey, WriteMap(settings.ToolbarRoles));
            this.store.Set(portalId, UploadFolderKey, NullIfEmpty(settings.UploadFolder));
            this.store.Set(portalId, DefaultUploadLimitKey, settings.DefaultUploadLimitKb?.ToString(CultureInfo.InvariantCulture));
            this.store.Set(portalId, UploadLimitRolesKey, WriteMap(settings.UploadLimitRoles));
        }

        private static string NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static int? ReadLimit(string value)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var limit) && limit > 0 ? limit : (int?)null;
        }

        private static IDictionary<string, T> ReadMap<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return new JavaScriptSerializer().Deserialize<Dictionary<string, T>>(json);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                // A hand-edited or damaged value is ignored, so the next scope's setting applies.
                return null;
            }
        }

        private static IDictionary<string, T> ToRoleMap<T>(IEnumerable<KeyValuePair<string, T>> entries, Func<T, bool> isValid)
        {
            if (entries == null)
            {
                return null;
            }

            // Role names are matched without regard to case; the last of any duplicates wins.
            var map = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries.Where(entry => !string.IsNullOrWhiteSpace(entry.Key) && isValid(entry.Value)))
            {
                map[entry.Key.Trim()] = entry.Value;
            }

            return map;
        }

        private static string WriteMap<T>(IDictionary<string, T> map)
        {
            return map == null || map.Count == 0 ? null : new JavaScriptSerializer().Serialize(map);
        }
    }
}
