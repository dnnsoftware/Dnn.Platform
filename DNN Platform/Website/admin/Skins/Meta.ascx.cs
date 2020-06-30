// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Web.UI.HtmlControls;

    /// -----------------------------------------------------------------------------
    /// <summary>A skin object which enables adding a <c>meta</c> element to the <c>head</c>.</summary>
    public partial class Meta : SkinObjectBase
    {
        /// <summary>Backing field for <see cref="Http" />.</summary>
        private readonly HttpPlaceholder http = new HttpPlaceholder();

        /// <summary>Gets an object to set the <see cref="HttpEquiv" /> property.</summary>
        public HttpPlaceholder Http
        {
            get { return this.http; }
        }

        /// <summary>
        /// Gets or sets the name of the <c>meta</c> element
        /// Either the name or the <see cref="HttpEquiv" /> must be set.
        /// The <c>name</c> attribute is not rendered if it is not set.
        /// </summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the content of the <c>meta</c> element.</summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the <c>http-equiv</c> attribute of the <c>meta</c> element.
        /// If specified, this is the name of the HTTP header that the
        /// <c>meta</c>
        /// The attribute is not rendered if it is not set.
        /// Either this or the <see cref="Name" /> must be set.
        /// </summary>
        public string HttpEquiv
        {
            get { return this.Http.Equiv; }
            set { this.Http.Equiv = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to insert this <c>meta</c>
        /// element at the beginning of the <c>head</c> element, rather than the
        /// end.
        /// </summary>
        public bool InsertFirst { get; set; }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // if(!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Content))
            // {
            //    var metaTag = new HtmlMeta();
            //    metaTag.Name = Name;
            //    metaTag.Content = Content;
            //    Page.Header.Controls.Add(metaTag);
            // }
            if ((!string.IsNullOrEmpty(this.Name) || !string.IsNullOrEmpty(this.HttpEquiv)) && !string.IsNullOrEmpty(this.Content))
            {
                var metaTag = new HtmlMeta();

                if (!string.IsNullOrEmpty(this.HttpEquiv))
                {
                    metaTag.HttpEquiv = this.HttpEquiv;
                }

                if (!string.IsNullOrEmpty(this.Name))
                {
                    metaTag.Name = this.Name;
                }

                metaTag.Content = this.Content;

                if (this.InsertFirst)
                {
                    this.Page.Header.Controls.AddAt(0, metaTag);
                }
                else
                {
                    this.Page.Header.Controls.Add(metaTag);
                }
            }
        }

        /// <summary>
        /// A class used by the <see cref="Http" /> property to enable setting
        /// the <see cref="HttpEquiv" /> property via <c>Http-Equiv</c> syntax
        /// in Web Forms markup.
        /// </summary>
        public class HttpPlaceholder
        {
            /// <summary>
            /// Gets or sets the <see cref="Meta.HttpEquiv"/> of the parent
            /// <c>meta</c> element.
            /// </summary>
            public string Equiv { get; set; }
        }
    }
}
