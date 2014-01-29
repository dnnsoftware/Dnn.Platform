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
using System.Web.UI.HtmlControls;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <remarks></remarks>
    /// <history>
    /// 	[cniknet]	10/15/2004	Replaced public members with properties and removed
    ///                             brackets from property names
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class Meta : SkinObjectBase
	{
		#region "Public Properties"

		public string Name { get; set; }

		public string Content { get; set; }

        public string HttpEquiv { get; set; }

        public bool InsertFirst { get; set; }

		#endregion

		#region "Event Handlers"

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

		#endregion
	}
}