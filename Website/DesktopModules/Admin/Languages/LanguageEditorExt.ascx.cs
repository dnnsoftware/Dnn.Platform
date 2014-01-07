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

using System;
using System.IO;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Modules.Admin.Languages
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Manages translations for Resource files
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[vmasanas]	10/04/2004  Created
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class LanguageEditorExt : PortalModuleBase
    {
		#region "Private Members"
		
        private string _highlight;
        private string _locale;
        private string _mode;
        private string _resfile;

		#endregion

		#region "Protected Properties"

        protected string ReturnUrl
        {
            get
            {
                return ModuleContext.NavigateUrl(TabId, "", true, "ctl=Editor", "mid=" + ModuleId, "Locale=" + _locale, "ResourceFile=" + Globals.QueryStringEncode(_resfile), "Mode=" + _mode, "Highlight=" + _highlight);
            }
        }

		#endregion

		#region "Private Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Loads resources from file 
        /// </summary>
        /// <param name="mode">Active editor mode</param>
        /// <param name="type">Resource being loaded (edit or default)</param>
        /// <returns></returns>
        /// <remarks>
        /// Depending on the editor mode, resources will be overrided using default DNN schema.
        /// "Edit" resources will only load selected file.
        /// When loading "Default" resources (to be used on the editor as helpers) fallback resource
        /// chain will be used in order for the editor to be able to correctly see what 
        /// is the current default value for the any key. This process depends on the current active
        /// editor mode:
        /// - System: when editing system base resources on en-US needs to be loaded
        /// - Host: base en-US, and base locale especific resource
        /// - Portal: base en-US, host override for en-US, base locale especific resource, and host override 
        /// for locale
        /// </remarks>
        /// <history>
        /// 	[vmasanas]	25/03/2006	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private string LoadFile(string mode, string type)
        {
            string file;
            string t = "";
            string temp;
            switch (type)
            {
                case "Edit":
                    //Only load resources from the file being edited
                    file = ResourceFile(_locale, mode);
                    temp = LoadResource(file);
                    if (temp != null)
                    {
                        t = temp;
                    }
                    break;
                case "Default":
                    //Load system default
                    file = ResourceFile(Localization.SystemLocale, "System");
                    t = LoadResource(file);
                    switch (mode)
                    {
                        case "Host":
                            if (_locale != Localization.SystemLocale)
                            {
								//Load base file for selected locale
                                file = ResourceFile(_locale, "System");
                                temp = LoadResource(file);
                                if (temp != null)
                                {
                                    t = temp;
                                }
                            }
                            break;
                        case "Portal":
                            //Load host override for default locale
                            file = ResourceFile(Localization.SystemLocale, "Host");
                            temp = LoadResource(file);
                            if (temp != null)
                            {
                                t = temp;
                            }
                            if (_locale != Localization.SystemLocale)
                            {
								//Load base file for locale
                                file = ResourceFile(_locale, "System");
                                temp = LoadResource(file);
                                if (temp != null)
                                {
                                    t = temp;
                                }
								
								//Load host override for selected locale
                                file = ResourceFile(_locale, "Host");
                                temp = LoadResource(file);
                                if (temp != null)
                                {
                                    t = temp;
                                }
                            }
                            break;
                    }
                    break;
            }
            return t;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Loads resource from file
        /// </summary>
        /// <param name="filepath">Resources file</param>
        /// <returns>Resource value</returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[vmasanas]	25/03/2006	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private string LoadResource(string filepath)
        {
            var d = new XmlDocument();
            bool xmlLoaded;
            string ret = null;
            try
            {
                d.Load(filepath);
                xmlLoaded = true;
            }
            catch
            {
                xmlLoaded = false;
            }
            if (xmlLoaded)
            {
                var node = d.SelectSingleNode("//root/data[@name='" + lblName.Text + "']/value");
                if (node != null)
                {
                    ret = node.InnerXml;
                }
            }
            return ret;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns the resource file name for a given resource and language
        /// </summary>
        /// <param name="language"></param>
        /// <param name="mode">Identifies the resource being searched (System, Host, Portal)</param>
        /// <returns>Localized File Name</returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[vmasanas]	04/10/2004	Created
        /// 	[vmasanas]	25/03/2006	Modified to support new host resources and incremental saving
        /// </history>
        /// -----------------------------------------------------------------------------
        private string ResourceFile(string language, string mode)
        {
            return Localization.GetResourceFileName(Server.MapPath("~\\" + _resfile), language, mode, PortalId);
        }

		#endregion

		#region "Event Handlers"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Loads resource file and default data
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[vmasanas]	07/10/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdCancel.Click += cmdCancel_Click;
            cmdUpdate.Click += cmdUpdate_Click;

            try
            {
                _resfile = Globals.QueryStringDecode(Request.QueryString["resourcefile"]);
                _locale = Request.QueryString["locale"];
                _mode = Request.QueryString["mode"];
                _highlight = Request.QueryString["highlight"];
                lblName.Text = Request.QueryString["name"];
                lblFile.Text = ResourceFile(_locale, _mode).Replace(Globals.ApplicationMapPath, "").Replace("\\", "/");
                if (!Page.IsPostBack)
                {
                    var defaultValue = LoadFile(_mode, "Default");
                    var editValue = LoadFile(_mode, "Edit");
                    if (string.IsNullOrEmpty(editValue))
                    {
                        editValue = defaultValue;
                    }
                    teContent.Text = editValue;
                    lblDefault.Text = Server.HtmlDecode(defaultValue);
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns to language editor control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[vmasanas]	04/10/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdCancel_Click(Object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(ReturnUrl, true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Saves the translation to the resource file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[vmasanas]	07/10/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdUpdate_Click(Object sender, EventArgs e)
        {
            XmlNode node;
            XmlNode nodeData;
            XmlNode parent;
            XmlAttribute attr;
            var resDoc = new XmlDocument();
            string filename;
            bool IsNewFile = false;
            try
            {
                if (String.IsNullOrEmpty(teContent.Text))
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("RequiredField.ErrorMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                    return;
                }
                filename = ResourceFile(_locale, _mode);
                if (!File.Exists(filename))
                {
					//load system default
                    resDoc.Load(ResourceFile(Localization.SystemLocale, "System"));
                    IsNewFile = true;
                }
                else
                {
                    resDoc.Load(filename);
                }
                switch (_mode)
                {
                    case "System":
                        node = resDoc.SelectSingleNode("//root/data[@name='" + lblName.Text + "']/value");
                        if (node == null)
                        {
							//missing entry
                            nodeData = resDoc.CreateElement("data");
                            attr = resDoc.CreateAttribute("name");
                            attr.Value = lblName.Text;
                            nodeData.Attributes.Append(attr);
                            resDoc.SelectSingleNode("//root").AppendChild(nodeData);

                            node = nodeData.AppendChild(resDoc.CreateElement("value"));
                        }
                        node.InnerXml = teContent.Text;

                        resDoc.Save(filename);
                        break;
                    case "Host":
                    case "Portal":
                        if (IsNewFile)
                        {
                            if (teContent.Text != lblDefault.Text)
                            {
                                foreach (XmlNode n in resDoc.SelectNodes("//root/data"))
                                {
                                    parent = n.ParentNode;
                                    parent.RemoveChild(n);
                                }
                                nodeData = resDoc.CreateElement("data");
                                attr = resDoc.CreateAttribute("name");
                                attr.Value = lblName.Text;
                                nodeData.Attributes.Append(attr);
                                resDoc.SelectSingleNode("//root").AppendChild(nodeData);

                                node = nodeData.AppendChild(resDoc.CreateElement("value"));
                                node.InnerXml = teContent.Text;

                                resDoc.Save(filename);
                            }
                        }
                        else
                        {
                            node = resDoc.SelectSingleNode("//root/data[@name='" + lblName.Text + "']/value");
                            if (teContent.Text != lblDefault.Text)
                            {
                                if (node == null)
                                {
                                    nodeData = resDoc.CreateElement("data");
                                    attr = resDoc.CreateAttribute("name");
                                    attr.Value = lblName.Text;
                                    nodeData.Attributes.Append(attr);
                                    resDoc.SelectSingleNode("//root").AppendChild(nodeData);

                                    node = nodeData.AppendChild(resDoc.CreateElement("value"));
                                }
                                node.InnerXml = teContent.Text;
                            }
                            else if (node != null)
                            {
								//remove item = default
                                resDoc.SelectSingleNode("//root").RemoveChild(node.ParentNode);
                            }
                            if (resDoc.SelectNodes("//root/data").Count > 0)
                            {
								//there's something to save
                                resDoc.Save(filename);
                            }
                            else if (File.Exists(filename))
                            {
								//nothing to be saved, if file exists delete
                                File.Delete(filename);
                            }
                        }
                        break;
                }
                Response.Redirect(ReturnUrl, true);
            }
            catch (Exception) //Module failed to load
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("Save.ErrorMessage", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
            }
			
			#endregion
        }
    }
}