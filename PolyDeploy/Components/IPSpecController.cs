using Cantarus.Modules.PolyDeploy.DataAccess.DataControllers;
using Cantarus.Modules.PolyDeploy.DataAccess.Models;

namespace Cantarus.Modules.PolyDeploy.Components
{
    public class IPSpecController
    {
        public static bool IsWhitelisted(string address)
        {
            IPSpecDataController dc = new IPSpecDataController();

            IPSpec ipSpec = dc.FindByAddress(address);

            return ipSpec != null;
        }
    }
}
