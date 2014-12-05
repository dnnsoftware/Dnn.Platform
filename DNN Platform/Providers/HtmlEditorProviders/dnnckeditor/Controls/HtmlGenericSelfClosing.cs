/*
 * CKEditor Html Editor Provider for DotNetNuke
 * ========
 * http://dnnckeditor.codeplex.com/
 * Copyright (C) Ingo Herbote
 *
 * The software, this file and its contents are subject to the CKEditor Provider
 * License. Please read the license.txt file before using, installing, copying,
 * modifying or distribute this file or part of its contents. The contents of
 * this file is part of the Source Code of the CKEditor Provider.
 */

namespace WatchersNET.CKEditor.Controls
{
    #region

    using System;
    using System.Collections;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    #endregion

    /// <summary>
    /// The html generic self closing.
    /// </summary>
    public class HtmlGenericSelfClosing : HtmlGenericControl
    {
        #region Constructors and Destructors

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

        #endregion

        #region Properties

        /// <summary>
        /// Gets Controls.
        /// </summary>
        /// <exception cref="Exception">A self closing tag cannot have child controls and/or content</exception>
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
        /// <exception cref="Exception">A self closing tag cannot have child controls and/or content</exception>
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
        /// <exception cref="Exception">A self closing tag cannot have child controls and/or content</exception>
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

        #endregion

        #region Methods

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

        #endregion
    }
}