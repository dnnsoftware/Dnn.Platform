// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

using DotNetNuke.Entities.Users;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <returns></returns>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    public partial class CurrentDate : SkinObjectBase
    {
        public string CssClass { get; set; }

        public string DateFormat { get; set; }

        private void InitializeComponent()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!String.IsNullOrEmpty(CssClass))
            {
                lblDate.CssClass = CssClass;
            }
            var user = UserController.Instance.GetCurrentUserInfo();
            lblDate.Text = !String.IsNullOrEmpty(DateFormat) ? user.LocalTime().ToString(DateFormat) : user.LocalTime().ToLongDateString();
        }
    }
}
