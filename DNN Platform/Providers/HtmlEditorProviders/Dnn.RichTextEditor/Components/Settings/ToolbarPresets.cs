// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Components.Settings
{
    using System;
    using System.Collections.Generic;

    /// <summary>The editor's toolbar presets, from fewest to most buttons.</summary>
    public static class ToolbarPresets
    {
        /// <summary>Formatting, lists, links and images.</summary>
        public const string Basic = "basic";

        /// <summary>The basic buttons plus headings, source view and full screen.</summary>
        public const string Standard = "standard";

        /// <summary>Every button.</summary>
        public const string Full = "full";

        /// <summary>Gets the presets, from fewest to most buttons.</summary>
        public static IReadOnlyList<string> All { get; } = new[] { Basic, Standard, Full, };

        /// <summary>Converts a preset name, in any case, to its canonical form.</summary>
        /// <param name="name">The name, e.g. <c>Full</c> (as the CKEditor provider stored it).</param>
        /// <returns>The preset, or <see langword="null"/> if the name is not a preset.</returns>
        public static string Normalize(string name)
        {
            foreach (var preset in All)
            {
                if (string.Equals(preset, name?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return preset;
                }
            }

            return null;
        }

        /// <summary>Gets the preset with the most buttons.</summary>
        /// <param name="presets">The presets.</param>
        /// <returns>The largest preset, or <see langword="null"/> if there are none.</returns>
        public static string Largest(IEnumerable<string> presets)
        {
            string largest = null;
            foreach (var preset in presets)
            {
                var normalized = Normalize(preset);
                if (normalized != null && (largest == null || IndexOf(normalized) > IndexOf(largest)))
                {
                    largest = normalized;
                }
            }

            return largest;
        }

        private static int IndexOf(string preset)
        {
            for (var index = 0; index < All.Count; index++)
            {
                if (All[index] == preset)
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
