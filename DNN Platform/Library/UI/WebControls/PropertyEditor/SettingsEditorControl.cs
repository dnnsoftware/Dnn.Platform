// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      SettingsEditorControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SettingsEditorControl control provides an Editor to edit DotNetNuke
    /// Settings
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:SettingsEditorControl runat=server></{0}:SettingsEditorControl>")]
    public class SettingsEditorControl : PropertyEditorControl
	{
		#region "Protected Properties"

		/// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Underlying DataSource
        /// </summary>
        /// <value>An IEnumerable</value>
        /// -----------------------------------------------------------------------------
        protected override IEnumerable UnderlyingDataSource
        {
            get
            {
                return GetSettings();
            }
        }

		#endregion

		#region "Public Properties"

		/// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the CustomEditors that are used by this control
        /// </summary>
        /// <value>The CustomEditors object</value>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public Hashtable CustomEditors { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Visibility values that are used by this control
        /// </summary>
        /// <value>The CustomEditors object</value>
        /// -----------------------------------------------------------------------------
        public Hashtable Visibility { get; set; }

		#endregion

		#region "Private Methods"

		/// -----------------------------------------------------------------------------
        /// <summary>
        /// GetSettings converts the DataSource into an ArrayList (IEnumerable)
        /// </summary>
        /// -----------------------------------------------------------------------------
        private ArrayList GetSettings()
        {
            var settings = (Hashtable) DataSource;
            var arrSettings = new ArrayList();
            IDictionaryEnumerator settingsEnumerator = settings.GetEnumerator();
            while (settingsEnumerator.MoveNext())
            {
                var info = new SettingInfo(settingsEnumerator.Key, settingsEnumerator.Value);
                if ((CustomEditors != null) && (CustomEditors[settingsEnumerator.Key] != null))
                {
                    info.Editor = Convert.ToString(CustomEditors[settingsEnumerator.Key]);
                }
                arrSettings.Add(info);
            }
            arrSettings.Sort(new SettingNameComparer());
            return arrSettings;
        }

		#endregion

		#region "Protected Override Methods"

		protected override void AddEditorRow(Table table, object obj)
        {
            var info = (SettingInfo) obj;
            AddEditorRow(table, info.Name, new SettingsEditorInfoAdapter(DataSource, obj, ID));
        }

        protected override void AddEditorRow(Panel container, object obj)
        {
            var info = (SettingInfo)obj;
            AddEditorRow(container, info.Name, new SettingsEditorInfoAdapter(DataSource, obj, ID));
        }

        protected override void AddEditorRow(object obj)
        {
            var info = (SettingInfo)obj; 
            AddEditorRow(this, info.Name, new SettingsEditorInfoAdapter(DataSource, obj, ID));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetRowVisibility determines the Visibility of a row in the table
        /// </summary>
        /// <param name="obj">The property</param>
        /// -----------------------------------------------------------------------------
        protected override bool GetRowVisibility(object obj)
        {
            var info = (SettingInfo) obj;
            bool _IsVisible = true;
            if ((Visibility != null) && (Visibility[info.Name] != null))
            {
                _IsVisible = Convert.ToBoolean(Visibility[info.Name]);
            }
            return _IsVisible;
		}

		#endregion
	}
}
