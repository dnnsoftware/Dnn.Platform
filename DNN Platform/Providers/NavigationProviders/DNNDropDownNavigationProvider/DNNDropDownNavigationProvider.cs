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
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Modules.NavigationProvider;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.NavigationControl
{
    public class DNNDropDownNavigationProvider : NavigationProvider
    {
        private DropDownList m_objDropDown;
        private string m_strControlID;

        public DropDownList DropDown
        {
            get
            {
                return m_objDropDown;
            }
        }

        public override Control NavigationControl
        {
            get
            {
                return DropDown;
            }
        }

        public override string ControlID
        {
            get
            {
                return m_strControlID;
            }
            set
            {
                m_strControlID = value;
            }
        }

        public override bool SupportsPopulateOnDemand
        {
            get
            {
                return false;
            }
        }

        public override string CSSControl
        {
            get
            {
                return DropDown.CssClass;
            }
            set
            {
                DropDown.CssClass = value;
            }
        }

        public override void Initialize()
        {
            m_objDropDown = new DropDownList();
            DropDown.ID = m_strControlID;
            DropDown.SelectedIndexChanged += DropDown_SelectedIndexChanged;
        }

        public override void Bind(DNNNodeCollection objNodes)
        {
            string strLevelPrefix;
            foreach (DNNNode objNode in objNodes)
            {
                if (objNode.Level == 0)
                {
                    DropDown.Items.Clear();
                }
                if (objNode.ClickAction == eClickAction.PostBack)
                {
                    DropDown.AutoPostBack = true;
                }
                strLevelPrefix = new string('_', objNode.Level);
                if (objNode.IsBreak)
                {
                    DropDown.Items.Add("-------------------");
                }
                else
                {
                    DropDown.Items.Add(new ListItem(strLevelPrefix + objNode.Text, objNode.ID));
                }
                Bind(objNode.DNNNodes);
            }
        }

        private void DropDown_SelectedIndexChanged(object source, EventArgs e)
        {
            if (DropDown.SelectedIndex > -1)
            {
                base.RaiseEvent_NodeClick(DropDown.SelectedItem.Value);
            }
        }
    }
}