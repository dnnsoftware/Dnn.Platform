using Cantarus.Modules.PolyDeploy.DataAccess.DataControllers;
using Cantarus.Modules.PolyDeploy.DataAccess.Models;
using System;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal class APIUserManager
    {
        private static APIUserManager _instance;

        public static APIUser Create(string name)
        {
            APIUser newApiUser;

            try
            {
                newApiUser = new APIUser(name);

                APIUserDataController dc = new APIUserDataController();

                dc.Create(newApiUser);
            }
            catch (Exception ex)
            {
                return null;
            }

            return newApiUser;
        }

        public static APIUser GetByAPIKey(string apiKey)
        {
            APIUserDataController dc = new APIUserDataController();

            return dc.Get(apiKey);
        }
    }
}
