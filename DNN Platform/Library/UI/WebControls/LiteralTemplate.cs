// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Web.UI;

#endregion

namespace DotNetNuke.UI.WebControls
{
    public class LiteralTemplate : ITemplate
    {
        private readonly Control m_objControl;
        private readonly string m_strHTML = "";

        public LiteralTemplate(string html)
        {
            m_strHTML = html;
        }

        public LiteralTemplate(Control ctl)
        {
            m_objControl = ctl;
        }

        #region ITemplate Members

        public void InstantiateIn(Control container)
        {
            if (m_objControl == null)
            {
                container.Controls.Add(new LiteralControl(m_strHTML));
            }
            else
            {
                container.Controls.Add(m_objControl);
            }
        }

        #endregion
    }
}
