using Cantarus.Modules.PolyDeploy.Components.DataAccess.DataControllers;
using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Net;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal static class IPSpecManager
    {
        private static IPSpecDataController IPSpecDC = new IPSpecDataController();

        /*
         * Create a new IPSpec object using the passed address.
         */
        public static IPSpec Create(string address)
        {
            IPSpec ipSpec;

            try
            {
                ipSpec = new IPSpec(address);

                IPSpecDC.Create(ipSpec);
            }
            catch (Exception ex)
            {
                return null;
            }

            return ipSpec;
        }

        /*
         * Retrieve all the IPSpec objects from the database.
         */
        public static IEnumerable<IPSpec> GetAll()
        {
            return IPSpecDC.Get();
        }

        /*
         * Get single IPSpec by its id.
         */
         public static IPSpec GetById(int id)
        {
            return IPSpecDC.Get(id);
        }

        /*
         * Check to see if the passed address is whitelisted.
         */
        public static bool IsWhitelisted(string address)
        {
            if (!IPAddress.TryParse(address, out _))
            {
                // see if address is an IP plus port, e.g. "1.1.1.1:58290"
                if (Uri.TryCreate(Uri.UriSchemeHttps + Uri.SchemeDelimiter + address, UriKind.Absolute, out Uri uri))
                {
                    if (uri.HostNameType != UriHostNameType.IPv4 && uri.HostNameType != UriHostNameType.IPv6)
                    {
                        return false;
                    }

                    address = uri.Host;
                }
            }

            IPSpec ipSpec = IPSpecDC.FindByAddress(address);

            return ipSpec != null;
        }

        /*
         * Delete the passed IPSpec.
         */
         public static void Delete(IPSpec ipSpec)
        {
            IPSpecDC.Delete(ipSpec);
        }
    }
}
