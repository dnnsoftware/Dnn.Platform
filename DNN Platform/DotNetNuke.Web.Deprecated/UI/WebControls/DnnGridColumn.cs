﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Web.UI.WebControls;

using DotNetNuke.Services.Localization;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnGridColumn : GridColumn
    {
        #region Public Properties

        public string LocalResourceFile
        {
            get
            {
                return Utilities.GetLocalResourceFile(Owner.OwnerGrid.Parent);
            }
        }

        #endregion

        #region Public Methods

        public override GridColumn Clone()
        {
            var dnnGridColumn = new DnnGridColumn();
            dnnGridColumn.CopyBaseProperties(this);
            dnnGridColumn.setHeaderText = HeaderText;
            return dnnGridColumn;
        }

        private String _HeaderText;

        public override string HeaderText
        {
            get
            {
                if (String.IsNullOrEmpty(base.HeaderText))
                    base.HeaderText = Localization.GetString(string.Format("{0}.Header", _HeaderText), DotNetNuke.Web.UI.Utilities.GetLocalResourceFile(Owner.OwnerGrid.Parent));
                return base.HeaderText;
            }
            set
            {
                _HeaderText = value;
                base.HeaderText = "";
            }
        }

        public String setHeaderText
        {
            set
            {
                base.HeaderText = value;
            }
        }


        #endregion
    }
}
