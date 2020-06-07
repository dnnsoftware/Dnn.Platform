using Cantarus.Modules.PolyDeploy.Components.DataAccess.DataControllers;
using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using Cantarus.Modules.PolyDeploy.Components.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal static class IPSpecManager
    {
        private static IPSpecDataController IPSpecDC = new IPSpecDataController();

        /// <summary>
        /// Create a new IPSpec object using the passed name and address.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public static IPSpec Create(string name, string address)
        {
            IPSpec ipSpec = IPSpecDC.GetByName(name);

            if (ipSpec != null)
            {
                throw new IPSpecExistsException($"An entry named '{ipSpec.Name}' already exists.");
            }

            ipSpec = IPSpecDC.Get(address);

            if (ipSpec != null)
            {
                throw new IPSpecExistsException($"IP '{address}' is already whitelisted by entry named '{ipSpec.Name}'.");
            }

            ipSpec = new IPSpec(name, address);

            IPSpecDC.Create(ipSpec);

            return ipSpec;
        }

        /// <summary>
        /// Retrieve all the IPSpec objects from the database.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IPSpec> GetAll()
        {
            return IPSpecDC.Get();
        }

        /// <summary>
        /// Get single IPSpec by its id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IPSpec GetById(int id)
        {
            return IPSpecDC.Get(id);
        }

        /// <summary>
        /// Check to see if the passed address is whitelisted.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
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

            IPSpec ipSpec = IPSpecDC.Get(address);

            return ipSpec != null;
        }

        /// <summary>
        /// Delete the passed IPSpec.
        /// </summary>
        /// <param name="ipSpec"></param>
        public static void Delete(IPSpec ipSpec)
        {
            IPSpecDC.Delete(ipSpec);
        }
    }
}
