﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System.Web.UI;

    public class LiteralTemplate : ITemplate
    {
        private readonly Control m_objControl;
        private readonly string m_strHTML = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralTemplate"/> class.
        /// </summary>
        /// <param name="html"></param>
        public LiteralTemplate(string html)
        {
            this.m_strHTML = html;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralTemplate"/> class.
        /// </summary>
        /// <param name="ctl"></param>
        public LiteralTemplate(Control ctl)
        {
            this.m_objControl = ctl;
        }

        /// <inheritdoc/>
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
