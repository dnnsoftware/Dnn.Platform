// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      SettingsEditorControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SettingsEditorControl control provides an Editor to edit DotNetNuke
    /// Settings.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:SettingsEditorControl runat=server></{0}:SettingsEditorControl>")]
    public class SettingsEditorControl : PropertyEditorControl
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the CustomEditors that are used by this control.
        /// </summary>
        /// <value>The CustomEditors object.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public Hashtable CustomEditors { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Visibility values that are used by this control.
        /// </summary>
        /// <value>The CustomEditors object.</value>
        /// -----------------------------------------------------------------------------
        public Hashtable Visibility { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Underlying DataSource.
        /// </summary>
        /// <value>An IEnumerable.</value>
        /// -----------------------------------------------------------------------------
        protected override IEnumerable UnderlyingDataSource
        {
            get
            {
                return this.GetSettings();
            }
        }

        protected override void AddEditorRow(Table table, object obj)
        {
            var info = (SettingInfo)obj;
            this.AddEditorRow(table, info.Name, new SettingsEditorInfoAdapter(this.DataSource, obj, this.ID));
        }

        protected override void AddEditorRow(Panel container, object obj)
        {
            var info = (SettingInfo)obj;
            this.AddEditorRow(container, info.Name, new SettingsEditorInfoAdapter(this.DataSource, obj, this.ID));
        }

        protected override void AddEditorRow(object obj)
        {
            var info = (SettingInfo)obj;
            this.AddEditorRow(this, info.Name, new SettingsEditorInfoAdapter(this.DataSource, obj, this.ID));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetRowVisibility determines the Visibility of a row in the table.
        /// </summary>
        /// <param name="obj">The property.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        protected override bool GetRowVisibility(object obj)
        {
            var info = (SettingInfo)obj;
            bool _IsVisible = true;
            if ((this.Visibility != null) && (this.Visibility[info.Name] != null))
            {
                _IsVisible = Convert.ToBoolean(this.Visibility[info.Name]);
            }

            return _IsVisible;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetSettings converts the DataSource into an ArrayList (IEnumerable).
        /// </summary>
        /// -----------------------------------------------------------------------------
        private ArrayList GetSettings()
        {
            var settings = (Hashtable)this.DataSource;
            var arrSettings = new ArrayList();
            IDictionaryEnumerator settingsEnumerator = settings.GetEnumerator();
            while (settingsEnumerator.MoveNext())
            {
                var info = new SettingInfo(settingsEnumerator.Key, settingsEnumerator.Value);
                if ((this.CustomEditors != null) && (this.CustomEditors[settingsEnumerator.Key] != null))
                {
                    info.Editor = Convert.ToString(this.CustomEditors[settingsEnumerator.Key]);
                }

                arrSettings.Add(info);
            }

            arrSettings.Sort(new SettingNameComparer());
            return arrSettings;
        }
    }
}
