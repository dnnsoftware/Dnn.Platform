#region Usings

using System;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;

using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.UI.WebControls;

using DataCache = DotNetNuke.Common.Utilities.DataCache;
using Globals = DotNetNuke.Common.Globals;
using System.Web.UI.WebControls;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Web.Client;

#endregion

namespace DotNetNuke.Modules.Admin.Users
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The emmpty user control used for users account page.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class Users : UserUserControlBase
    {
    }
}
