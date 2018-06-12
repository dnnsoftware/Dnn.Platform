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

#region Usings

using System;
using System.Web.UI;
using DotNetNuke.Web.UI;
using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
   public class DnnToolTip : RadToolTip, ILocalizable
   {
      private bool _localize = true;

      protected override void Render(HtmlTextWriter writer)
      {
         LocalizeStrings();
         base.Render(writer);
      }

      public string ResourceKey { get; set; }

#region ILocalizable Implementation
      public bool Localize
      {
         get
         {
            if (base.DesignMode)
            {
               return false;
            }
            return _localize;
         }
         set
         {
            _localize = value;
         }
      }

      public string LocalResourceFile { get; set; }

      public virtual void LocalizeStrings()
      {
         if ((this.Localize) && (!(String.IsNullOrEmpty(this.ResourceKey))))
         {
            if (!(String.IsNullOrEmpty(base.ManualCloseButtonText)))
            {
               base.ManualCloseButtonText = Utilities.GetLocalizedStringFromParent(String.Format("{0}.ManualCloseButtonText", this.ResourceKey), this);
            }

            if (!(String.IsNullOrEmpty(base.Text)))
            {
               base.Text = Utilities.GetLocalizedStringFromParent(String.Format("{0}.Text", this.ResourceKey), this);
            }

            if (!(String.IsNullOrEmpty(base.ToolTip)))
            {
               base.ToolTip = Utilities.GetLocalizedStringFromParent(String.Format("{0}.ToolTip", this.ResourceKey), this);
            }
         }
      }
#endregion
   }
}
