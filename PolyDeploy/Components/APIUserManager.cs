using Cantarus.Modules.PolyDeploy.DataAccess.DataControllers;
using Cantarus.Modules.PolyDeploy.DataAccess.Models;
using System;
using System.Collections.Generic;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal class APIUserManager
    {
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

        public static IEnumerable<APIUser> GetAll()
        {
            APIUserDataController dc = new APIUserDataController();

            return dc.Get();
        }

        public static APIUser GetByAPIKey(string apiKey)
        {
            APIUserDataController dc = new APIUserDataController();

            return dc.Get(apiKey);
        }

        public static APIUser Update(APIUser apiUser)
        {
            APIUserDataController dc = new APIUserDataController();

            dc.Update(apiUser);

            return dc.Get(apiUser.APIUserId);
        }

        public static void Delete(APIUser apiuser)
        {
            APIUserDataController dc = new APIUserDataController();

            dc.Delete(apiuser);
        }
    }
}
