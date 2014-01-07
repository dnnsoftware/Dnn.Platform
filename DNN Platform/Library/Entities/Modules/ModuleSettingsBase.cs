#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using System.Collections;
using System.ComponentModel;

using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.Entities.Modules
{
    public class ModuleSettingsBase : PortalModuleBase, ISettingsControl
    {
        private Hashtable _moduleSettings;
        private Hashtable _settings;
        private Hashtable _tabModuleSettings;

        public Hashtable ModuleSettings
        {
            get
            {
                if (_moduleSettings == null)
                {
					//Get ModuleSettings
                    _moduleSettings = new ModuleController().GetModuleSettings(ModuleId);
                }
                return _moduleSettings;
            }
        }

        public Hashtable TabModuleSettings
        {
            get
            {
                if (_tabModuleSettings == null)
                {
					//Get TabModuleSettings
                    _tabModuleSettings = new ModuleController().GetTabModuleSettings(TabModuleId);
                }
                return _tabModuleSettings;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Hashtable Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new Hashtable();
					//Merge the TabModuleSettings and ModuleSettings
                    foreach (string strKey in TabModuleSettings.Keys)
                    {
                        _settings[strKey] = TabModuleSettings[strKey];
                    }
                    foreach (string strKey in ModuleSettings.Keys)
                    {
                        _settings[strKey] = ModuleSettings[strKey];
                    }
                }
                return _settings;
            }
        }

        #region ISettingsControl Members

        public virtual void LoadSettings()
        {
        }

        public virtual void UpdateSettings()
        {
        }

        #endregion
    }
}