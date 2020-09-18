// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System.Web.UI;

    public class LiteralTemplate : ITemplate
    {
        private readonly Control m_objControl;
        private readonly string m_strHTML = string.Empty;

        public LiteralTemplate(string html)
        {
            this.m_strHTML = html;
        }

        public LiteralTemplate(Control ctl)
        {
            this.m_objControl = ctl;
        }

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
    }
}
