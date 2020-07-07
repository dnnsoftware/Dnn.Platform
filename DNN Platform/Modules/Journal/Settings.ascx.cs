// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal
{
    /*
' Copyright (c) 2011  DotNetNuke Corporation
'  All rights reserved.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
'
*/

    using System;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Modules.Journal.Components;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Services.Journal.Internal;
    using DotNetNuke.Services.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Settings class manages Module Settings.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Settings : JournalSettingsBase
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadSettings loads the settings from the Database and displays them.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void LoadSettings()
        {
            try
            {
                if (this.Page.IsPostBack == false)
                {
                    this.BindJournalTypes();

                    // Check for existing settings and use those on this page
                    if (this.Settings.ContainsKey(Constants.DefaultPageSize))
                    {
                        this.drpDefaultPageSize.SelectedIndex = this.drpDefaultPageSize.Items.IndexOf(this.drpDefaultPageSize.Items.FindByValue(this.Settings[Constants.DefaultPageSize].ToString()));
                    }
                    else
                    {
                        this.drpDefaultPageSize.SelectedIndex = this.drpDefaultPageSize.Items.IndexOf(this.drpDefaultPageSize.Items.FindByValue("20"));
                    }

                    if (this.Settings.ContainsKey(Constants.MaxCharacters))
                    {
                        this.drpMaxMessageLength.SelectedIndex = this.drpMaxMessageLength.Items.IndexOf(this.drpMaxMessageLength.Items.FindByValue(this.Settings[Constants.MaxCharacters].ToString()));
                    }
                    else
                    {
                        this.drpMaxMessageLength.SelectedIndex = this.drpMaxMessageLength.Items.IndexOf(this.drpMaxMessageLength.Items.FindByValue("250"));
                    }

                    if (this.Settings.ContainsKey(Constants.AllowFiles))
                    {
                        this.chkAllowFiles.Checked = Convert.ToBoolean(this.Settings[Constants.AllowFiles].ToString());
                    }
                    else
                    {
                        this.chkAllowFiles.Checked = true;
                    }

                    if (this.Settings.ContainsKey(Constants.AllowPhotos))
                    {
                        this.chkAllowPhotos.Checked = Convert.ToBoolean(this.Settings[Constants.AllowPhotos].ToString());
                    }
                    else
                    {
                        this.chkAllowPhotos.Checked = true;
                    }

                    if (this.Settings.ContainsKey(Constants.AllowResizePhotos))
                    {
                        this.chkAllowResize.Checked = Convert.ToBoolean(this.Settings[Constants.AllowResizePhotos].ToString());
                    }
                    else
                    {
                        this.chkAllowResize.Checked = false;
                    }

                    if (this.Settings.ContainsKey(Constants.JournalEditorEnabled))
                    {
                        this.chkEnableEditor.Checked = Convert.ToBoolean(this.Settings[Constants.JournalEditorEnabled].ToString());
                    }
                    else
                    {
                        this.chkEnableEditor.Checked = true;
                    }

                    if (!this.chkEnableEditor.Checked)
                    {
                        this.chkAllowFiles.Enabled = false;
                        this.chkAllowPhotos.Enabled = false;
                    }

                    this.chkAllowResize.Enabled = this.chkEnableEditor.Checked && this.chkAllowPhotos.Checked;

                    foreach (ListItem li in this.chkJournalFilters.Items)
                    {
                        li.Selected = true;
                    }

                    if (this.Settings.ContainsKey(Constants.JournalFilters))
                    {
                        if (string.IsNullOrEmpty(this.Settings[Constants.JournalFilters].ToString()))
                        {
                            foreach (ListItem li in this.chkJournalFilters.Items)
                            {
                                li.Selected = true;
                            }
                        }
                        else
                        {
                            foreach (ListItem li in this.chkJournalFilters.Items)
                            {
                                li.Selected = false;
                            }

                            foreach (string s in this.Settings[Constants.JournalFilters].ToString().Split(';'))
                            {
                                foreach (ListItem li in this.chkJournalFilters.Items)
                                {
                                    if (li.Value == s)
                                    {
                                        li.Selected = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateSettings saves the modified settings to the Database.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UpdateSettings()
        {
            try
            {
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, Constants.DefaultPageSize, this.drpDefaultPageSize.SelectedItem.Value);
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, Constants.AllowFiles, this.chkAllowFiles.Checked.ToString());
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, Constants.AllowPhotos, this.chkAllowPhotos.Checked.ToString());
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, Constants.AllowResizePhotos, this.chkAllowResize.Checked.ToString());
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, Constants.JournalEditorEnabled, this.chkEnableEditor.Checked.ToString());
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, Constants.MaxCharacters, this.drpMaxMessageLength.SelectedItem.Value);
                string journalTypes = string.Empty;
                bool allTypes = true;
                foreach (ListItem li in this.chkJournalFilters.Items)
                {
                    if (!li.Selected)
                    {
                        allTypes = false;
                    }
                }

                var jc = InternalJournalController.Instance;
                jc.DeleteFilters(this.PortalId, this.ModuleId);

                foreach (ListItem li in this.chkJournalFilters.Items)
                {
                    if (li.Selected)
                    {
                        if (!allTypes)
                        {
                            jc.SaveFilters(this.PortalId, this.ModuleId, Convert.ToInt32(li.Value));
                            journalTypes += li.Value + ";";
                        }
                    }
                }

                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, Constants.JournalFilters, journalTypes);
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void BindJournalTypes()
        {
            foreach (JournalTypeInfo journalTypeInfo in JournalController.Instance.GetJournalTypes(this.PortalId))
            {
                this.chkJournalFilters.Items.Add(new ListItem(Localization.GetString(journalTypeInfo.JournalType, "~/desktopmodules/journal/app_localresources/sharedresources.resx"), journalTypeInfo.JournalTypeId.ToString()));
            }
        }
    }
}
