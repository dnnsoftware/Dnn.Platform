#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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
