using System.Web.UI;
using System.Collections.Generic;
using DotNetNuke.UI.Modules;
using DotNetNuke.Services.Tokens;

namespace DotNetNuke.Entities.Modules
{
    public interface ICustomTokenProvider
    {
        IDictionary<string, IPropertyAccess> GetTokens(Page page, ModuleInstanceContext moduleContext);
    }
}
