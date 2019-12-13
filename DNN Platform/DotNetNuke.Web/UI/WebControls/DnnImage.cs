// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{

    public class DnnImage : Image
    {        

        #region Public Properties

        public string IconKey { get; set; }
        public string IconSize { get; set; }
        public string IconStyle { get; set; }

        #endregion

        #region "Protected Methods"

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (string.IsNullOrEmpty(ImageUrl))
                ImageUrl = Entities.Icons.IconController.IconURL(IconKey, IconSize, IconStyle);            
        }

        #endregion

    }
}
