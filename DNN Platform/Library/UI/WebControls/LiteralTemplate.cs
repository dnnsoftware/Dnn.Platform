// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            this.m_strHTML = html;
        }

        public LiteralTemplate(Control ctl)
        {
            this.m_objControl = ctl;
        }

        #region ITemplate Members

        public void InstantiateIn(Control container)
        {
            if (this.m_objControl == null)
            {
                container.Controls.Add(new LiteralControl(this.m_strHTML));
            }
            else
            {
                container.Controls.Add(this.m_objControl);
            }
        }

        #endregion
    }
}
