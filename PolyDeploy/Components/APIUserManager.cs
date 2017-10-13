using Cantarus.Modules.PolyDeploy.Components.DataAccess.DataControllers;
using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using System;
using System.Collections.Generic;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal static class APIUserManager
    {
        private static APIUserDataController APIUserDC = new APIUserDataController();

        public static APIUser Create(string name)
        {
            APIUser newApiUser;

            try
            {
                newApiUser = new APIUser(name);

                APIUserDC.Create(newApiUser);
            }
            catch (Exception ex)
            {
                return null;
            }

            return newApiUser;
        }

        public static IEnumerable<APIUser> GetAll()
        {
            return APIUserDC.Get();
        }

        public static APIUser GetById(int id)
        {
            return APIUserDC.Get(id);
        }

        public static APIUser GetByAPIKey(string apiKey)
        {
            return APIUserDC.Get(apiKey);
        }

        public static APIUser Update(APIUser apiUser)
        {
            APIUserDC.Update(apiUser);

            return APIUserDC.Get(apiUser.APIUserId);
        }

        public static void Delete(APIUser apiUser)
        {
            APIUserDC.Delete(apiUser);
        }
    }
}
