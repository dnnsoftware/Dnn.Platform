// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Users
{
    using System;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Exceptions;

    public partial class ViewProfile : UserModuleBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.UserId = Null.NullInteger;
            if (this.Context.Request.QueryString["userticket"] != null)
            {
                this.UserId = int.Parse(UrlUtils.DecryptParameter(this.Context.Request.QueryString["userticket"]));
            }

            this.ctlProfile.ID = "Profile";
            this.ctlProfile.UserId = this.UserId;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (this.ctlProfile.UserProfile == null)
                {
                    this.lblNoProperties.Visible = true;
                    return;
                }

                this.ctlProfile.DataBind();
                if (this.ctlProfile.UserProfile.ProfileProperties.Cast<ProfilePropertyDefinition>().Count(profProperty => profProperty.Visible) == 0)
                {
                    this.lblNoProperties.Visible = true;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
