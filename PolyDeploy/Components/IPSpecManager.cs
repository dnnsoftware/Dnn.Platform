using Cantarus.Modules.PolyDeploy.Components.DataAccess.DataControllers;
using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using System;
using System.Collections.Generic;

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
