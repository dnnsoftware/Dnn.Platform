// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNNConnect.CKEditorProvider.Controls
{
    using System;
    using System.Collections;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    /// <summary>
    /// The html generic self closing.
    /// </summary>
    public class HtmlGenericSelfClosing : HtmlGenericControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlGenericSelfClosing"/> class.
        /// </summary>
        public HtmlGenericSelfClosing()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlGenericSelfClosing"/> class.
        /// </summary>
        /// <param name="sTag">
        /// The s tag.
        /// </param>
        public HtmlGenericSelfClosing(string sTag)
            : base(sTag)
        {
        }

        /// <summary>
        /// Gets Controls.
        /// </summary>
        /// <exception cref="Exception">A self closing tag cannot have child controls and/or content.</exception>
        public override ControlCollection Controls
        {
            get
            {
                throw new Exception("A self closing tag cannot have child controls and/or content");
            }
        }

        /// <summary>
        /// Gets or sets InnerHtml.
        /// </summary>
        /// <exception cref="Exception">A self closing tag cannot have child controls and/or content.</exception>
        public override string InnerHtml
        {
            get
            {
                return null;
            }

            set
            {
                throw new Exception("A self closing tag cannot have child controls and/or content");
            }
        }

        /// <summary>
        /// Gets or sets InnerText.
        /// </summary>
        /// <exception cref="Exception">A self closing tag cannot have child controls and/or content.</exception>
        public override string InnerText
        {
            get
            {
                return null;
            }

            set
            {
                throw new Exception("A self closing tag cannot have child controls and/or content");
            }
        }

        /// <summary>
        /// The render.
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        protected override void Render(HtmlTextWriter writer)
        {
            ICollection keys = this.Attributes.Keys;

            writer.WriteBeginTag(this.TagName);

            if (this.ID != string.Empty)
            {
                // writer.WriteAttribute("id", base.UniqueID);
                // writer.WriteAttribute("name", base.UniqueID);
            }

            foreach (string key in keys)
            {
                writer.WriteAttribute(key, this.Attributes[key]);
            }

            writer.Write(HtmlTextWriter.SelfClosingTagEnd + Environment.NewLine);
        }
    }
}
