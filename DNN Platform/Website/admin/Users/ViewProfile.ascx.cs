// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Linq;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;

#endregion

namespace DotNetNuke.Modules.Admin.Users
{
    public partial class ViewProfile : UserModuleBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            UserId = Null.NullInteger;
            if (Context.Request.QueryString["userticket"] != null)
            {
                UserId = Int32.Parse(UrlUtils.DecryptParameter(Context.Request.QueryString["userticket"]));
            }
            ctlProfile.ID = "Profile";
            ctlProfile.UserId = UserId;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (ctlProfile.UserProfile == null)
                {
                    lblNoProperties.Visible = true;
                    return;
                }
                ctlProfile.DataBind();
                if (ctlProfile.UserProfile.ProfileProperties.Cast<ProfilePropertyDefinition>().Count(profProperty => profProperty.Visible) == 0)
                {
                    lblNoProperties.Visible = true;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
