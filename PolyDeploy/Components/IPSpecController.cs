using Cantarus.Modules.PolyDeploy.Components.DataAccess.DataControllers;
using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using System;

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

        public static IPSpec AddWhitelistIp(string address)
        {
            IPSpec ipSpec;

            try
            {
                IPSpecDataController dc = new IPSpecDataController();

                ipSpec = new IPSpec(address);

                dc.Create(ipSpec);
            }
            catch (Exception ex)
            {
                return null;
            }

            return ipSpec;
        }
    }
}
