// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Components.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The editor's settings at one scope (all sites, or one site), or the effective settings for a site.
    /// A <see langword="null"/> value means the setting is not set at this scope.
    /// </summary>
    public class EditorSettings
    {
        /// <summary>Gets or sets the toolbar preset for users not in any role listed in <see cref="ToolbarRoles"/>.</summary>
        public string DefaultToolbar { get; set; }

        /// <summary>Gets or sets the toolbar preset for each role name. A user in several roles gets the largest preset.</summary>
        public IDictionary<string, string> ToolbarRoles { get; set; }

        /// <summary>Gets or sets the folder path that files dropped or pasted into the editor are uploaded to, e.g. <c>Images/</c>.</summary>
        public string UploadFolder { get; set; }

        /// <summary>Gets or sets the upload size limit in KB for users not in any role listed in <see cref="UploadLimitRoles"/>.</summary>
        public int? DefaultUploadLimitKb { get; set; }

        /// <summary>Gets or sets the upload size limit in KB for each role name. A user in several roles gets the largest limit.</summary>
        public IDictionary<string, int> UploadLimitRoles { get; set; }

        /// <summary>Gets the toolbar preset for the current user.</summary>
        /// <param name="isInRole">Returns whether the current user is in a role, given its name.</param>
        /// <param name="isAdministrator">Whether the current user is a superuser or site administrator.</param>
        /// <returns>The preset.</returns>
        public string ResolveToolbar(Func<string, bool> isInRole, bool isAdministrator)
        {
            if (this.ToolbarRoles != null && this.ToolbarRoles.Count > 0)
            {
                var largest = ToolbarPresets.Largest(this.ToolbarRoles.Where(role => isInRole(role.Key)).Select(role => role.Value));
                if (largest != null)
                {
                    return largest;
                }
            }
            else if (isAdministrator)
            {
                // Without role settings, administrators get the full toolbar, as with the CKEditor provider.
                return ToolbarPresets.Full;
            }

            return ToolbarPresets.Normalize(this.DefaultToolbar) ?? ToolbarPresets.Basic;
        }

        /// <summary>Gets the largest file the current user may upload.</summary>
        /// <param name="isInRole">Returns whether the current user is in a role, given its name.</param>
        /// <returns>The limit in bytes, or <see langword="null"/> for no limit beyond the site's maximum request size.</returns>
        public long? ResolveUploadLimitBytes(Func<string, bool> isInRole)
        {
            var limitsKb = (this.UploadLimitRoles ?? new Dictionary<string, int>())
                .Where(role => isInRole(role.Key))
                .Select(role => (int?)role.Value)
                .ToList();
            var limitKb = limitsKb.Count > 0 ? limitsKb.Max() : this.DefaultUploadLimitKb;
            return limitKb.HasValue ? limitKb.Value * 1024L : (long?)null;
        }
    }
}
