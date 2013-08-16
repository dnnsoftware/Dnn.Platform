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
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Modules.Journal.Components;
using DotNetNuke.Services.Journal;
using System.Web.UI.WebControls;

using DotNetNuke.Services.Journal.Internal;
using DotNetNuke.Services.Localization;


namespace DotNetNuke.Modules.Journal {

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Settings class manages Module Settings
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Settings : JournalSettingsBase {

        #region Base Method Implementations

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadSettings loads the settings from the Database and displays them
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void LoadSettings() {
            try {
                if (Page.IsPostBack == false) {
                    BindJournalTypes();
                    //Check for existing settings and use those on this page
                    if (Settings.ContainsKey(Constants.DefaultPageSize)) {
                        drpDefaultPageSize.SelectedIndex = drpDefaultPageSize.Items.IndexOf(drpDefaultPageSize.Items.FindByValue(Settings[Constants.DefaultPageSize].ToString()));
                    } else {
                        drpDefaultPageSize.SelectedIndex = drpDefaultPageSize.Items.IndexOf(drpDefaultPageSize.Items.FindByValue("20"));
                        
                    }
                    if (Settings.ContainsKey(Constants.MaxCharacters)) {
                        drpMaxMessageLength.SelectedIndex = drpMaxMessageLength.Items.IndexOf(drpMaxMessageLength.Items.FindByValue(Settings[Constants.MaxCharacters].ToString()));
                    } else {
                        drpMaxMessageLength.SelectedIndex = drpMaxMessageLength.Items.IndexOf(drpMaxMessageLength.Items.FindByValue("250"));

                    }
                    if (Settings.ContainsKey(Constants.AllowFiles)) {
                        chkAllowFiles.Checked = Convert.ToBoolean(Settings[Constants.AllowFiles].ToString());
                    } else {
                        chkAllowFiles.Checked = true;
                    }
                    if (Settings.ContainsKey(Constants.AllowPhotos)) {
                        chkAllowPhotos.Checked = Convert.ToBoolean(Settings[Constants.AllowPhotos].ToString());
                    } else {
                        chkAllowPhotos.Checked = true;
                    }
                    if (Settings.ContainsKey(Constants.JournalEditorEnabled))
                    {
                        chkEnableEditor.Checked = Convert.ToBoolean(Settings[Constants.JournalEditorEnabled].ToString());
                    } else
                    {
                        chkEnableEditor.Checked = true;
                    }
                    if (chkEnableEditor.Checked == false)
                    {
                        chkAllowFiles.Enabled = false;
                        chkAllowPhotos.Enabled = false;
                    }
                    foreach (ListItem li in chkJournalFilters.Items) {
                        li.Selected = true;
                    }
                    if (Settings.ContainsKey(Constants.JournalFilters)) {
                        if (String.IsNullOrEmpty(Settings[Constants.JournalFilters].ToString())) {
                            foreach (ListItem li in chkJournalFilters.Items) {
                                li.Selected = true;
                            }
                        } else {
                            foreach (ListItem li in chkJournalFilters.Items) {
                                li.Selected = false;
                            }
                            foreach (string s in Settings[Constants.JournalFilters].ToString().Split(';')) {
                                foreach (ListItem li in chkJournalFilters.Items) {
                                    if (li.Value == s) {
                                        li.Selected = true;
                                    }
                                }
                            }
                        }
                    }

                }
            } catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateSettings saves the modified settings to the Database
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UpdateSettings() {
            try {
                ModuleController modules = new ModuleController();
                modules.UpdateModuleSetting(this.ModuleId, Constants.DefaultPageSize, drpDefaultPageSize.SelectedItem.Value);
                modules.UpdateModuleSetting(this.ModuleId, Constants.AllowFiles, chkAllowFiles.Checked.ToString());
                modules.UpdateModuleSetting(this.ModuleId, Constants.AllowPhotos, chkAllowPhotos.Checked.ToString());
                modules.UpdateModuleSetting(this.ModuleId, Constants.JournalEditorEnabled, chkEnableEditor.Checked.ToString());
                modules.UpdateModuleSetting(this.ModuleId, Constants.MaxCharacters, drpMaxMessageLength.SelectedItem.Value);
                string journalTypes = "";
                bool allTypes = true;
                foreach (ListItem li in chkJournalFilters.Items) {
                    if (!li.Selected) {
                        allTypes = false;
                    }
                }
                var jc = InternalJournalController.Instance;
                jc.DeleteFilters(PortalId, ModuleId);

                foreach (ListItem li in chkJournalFilters.Items) {
                    if (li.Selected) {
                        if (!allTypes) {
                            jc.SaveFilters(PortalId, ModuleId, Convert.ToInt32(li.Value));
                            journalTypes += li.Value + ";";
                        }
                        
                    }
                }
                modules.UpdateModuleSetting(this.ModuleId, Constants.JournalFilters, journalTypes);

            } catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion
        #region Private Methods
        private void BindJournalTypes() {
            foreach (JournalTypeInfo journalTypeInfo in JournalController.Instance.GetJournalTypes(PortalId))
            {
                chkJournalFilters.Items.Add(new ListItem(Localization.GetString(journalTypeInfo.JournalType, "~/desktopmodules/journal/app_localresources/sharedresources.resx"), journalTypeInfo.JournalTypeId.ToString()));
            }
        }
        #endregion
    }

}

