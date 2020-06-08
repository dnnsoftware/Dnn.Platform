// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Web.UI.WebControls;

using DotNetNuke.Services.Localization;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnGridButtonColumn : GridButtonColumn
    {
        #region "Public Properties"

        public string LocalResourceFile
        {
            get
            {
                return Utilities.GetLocalResourceFile(this.Owner.OwnerGrid.Parent);
            }
        }

        #endregion

        #region "Public Methods"

        public override GridColumn Clone()
        {
            DnnGridButtonColumn dnnGridColumn = new DnnGridButtonColumn();

            //you should override CopyBaseProperties if you have some column specific properties
            dnnGridColumn.CopyBaseProperties(this);

            return dnnGridColumn;
        }

        public override void InitializeCell(TableCell cell, int columnIndex, GridItem inItem)
        {
            base.InitializeCell(cell, columnIndex, inItem);
            if (inItem is GridHeaderItem && !String.IsNullOrEmpty(this.HeaderText))
            {
                cell.Text = Localization.GetString(string.Format("{0}.Header", this.HeaderText), this.LocalResourceFile);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Icon Key to obtain ImageURL
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string IconKey { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Icon Siz to obtain ImageURL
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string IconSize { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Icon Style to obtain ImageURL
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string IconStyle { get; set; }

        public override string ImageUrl
        {
            get
            {
                if (string.IsNullOrEmpty(base.ImageUrl))
                    base.ImageUrl = Entities.Icons.IconController.IconURL(this.IconKey, this.IconSize, this.IconStyle);

                return base.ImageUrl;
            }
            set
            {
                base.ImageUrl = value;
            }
        }

        #endregion
    }
}
