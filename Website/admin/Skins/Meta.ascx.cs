#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Web.UI.HtmlControls;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    /// -----------------------------------------------------------------------------
    /// <summary>A skin object which enables adding a <c>meta</c> element to the <c>head</c></summary>
    public partial class Meta : SkinObjectBase
    {
        /// <summary>Backing field for <see cref="Http" /></summary>
        private readonly HttpPlaceholder http = new HttpPlaceholder();

        /// <summary>
        /// Gets or sets the name of the <c>meta</c> element
        /// Either the name or the <see cref="HttpEquiv" /> must be set.
        /// The <c>name</c> attribute is not rendered if it is not set.
        /// </summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the content of the <c>meta</c> element</summary>
        public string Content { get; set; }

        /// <summary>Gets an object to set the <see cref="HttpEquiv" /> property</summary>
        public HttpPlaceholder Http
        {
            get { return this.http; }
        }

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

            //if(!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Content))
            //{
            //    var metaTag = new HtmlMeta();
            //    metaTag.Name = Name;
            //    metaTag.Content = Content;
            //    Page.Header.Controls.Add(metaTag);
            //}

            if ((!string.IsNullOrEmpty(Name) || !string.IsNullOrEmpty(HttpEquiv)) && !string.IsNullOrEmpty(Content))
            {
                var metaTag = new HtmlMeta();

                if (!string.IsNullOrEmpty(HttpEquiv))
                    metaTag.HttpEquiv = HttpEquiv;
                if (!string.IsNullOrEmpty(Name))
                    metaTag.Name = Name;

                metaTag.Content = Content;

                if (InsertFirst)
                    Page.Header.Controls.AddAt(0, metaTag);
                else
                    Page.Header.Controls.Add(metaTag);
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
            /// <c>meta</c> element
            /// </summary>
            public string Equiv { get; set; }
        }
    }
}