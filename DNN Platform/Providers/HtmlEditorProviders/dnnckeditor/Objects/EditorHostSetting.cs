/*
 * CKEditor Html Editor Provider for DotNetNuke
 * ========
 * http://dnnckeditor.codeplex.com/
 * Copyright (C) Ingo Herbote
 *
 * The software, this file and its contents are subject to the CKEditor Provider
 * License. Please read the license.txt file before using, installing, copying,
 * modifying or distribute this file or part of its contents. The contents of
 * this file is part of the Source Code of the CKEditor Provider.
 */

namespace WatchersNET.CKEditor.Objects
{
    /// <summary>
    /// A Editor Host Setting Item
    /// </summary>
    public class EditorHostSetting
    {
        #region Properties

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorHostSetting"/> class.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        public EditorHostSetting(string settingName, string settingValue)
        {
            this.Name = settingName;
            this.Value = settingValue;
        }

        /// <summary>
        /// Gets or sets the setting name.
        /// </summary>
        /// <value>
        /// The setting name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the setting value.
        /// </summary>
        /// <value>
        /// The setting value.
        /// </value>
        public string Value { get; set; }
        
        #endregion
    }
}