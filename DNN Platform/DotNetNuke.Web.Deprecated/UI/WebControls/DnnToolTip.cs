// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
