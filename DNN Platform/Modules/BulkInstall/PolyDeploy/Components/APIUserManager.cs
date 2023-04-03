using Cantarus.Modules.PolyDeploy.Components.DataAccess.DataControllers;
using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using System;
using System.Collections.Generic;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal static class APIUserManager
    {
        private static APIUserDataController APIUserDC = new APIUserDataController();

        /// <summary>
        /// Creates a new APIUser with the passed name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static APIUser Create(string name)
        {
            return Create(name, bypass: false);
        }

        public static APIUser Create(string name, bool bypass)
        {
            APIUser newApiUser;

            try
            {
                newApiUser = new APIUser(name, bypass);

                APIUserDC.Create(newApiUser);
            }
            catch (Exception ex)
            {
                return null;
            }

            return newApiUser;
        }

        /// <summary>
        /// Gets all APIUsers.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<APIUser> GetAll()
        {
            return APIUserDC.Get();
        }

        /// <summary>
        /// Retrieves a single APIUser by its id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static APIUser GetById(int id)
        {
            return APIUserDC.Get(id);
        }

        /// <summary>
        /// Retrieves a single APIUser by its api key.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public static APIUser GetByAPIKey(string apiKey)
        {
            return APIUserDC.Get(apiKey);
        }

        /// <summary>
        /// Updates the passed APIUser.
        /// </summary>
        /// <param name="apiUser"></param>
        /// <returns></returns>
        public static APIUser Update(APIUser apiUser)
        {
            APIUserDC.Update(apiUser);

            return APIUserDC.Get(apiUser.APIUserId);
        }

        /// <summary>
        /// Deletes the passed APIUser.
        /// </summary>
        /// <param name="apiUser"></param>
        public static void Delete(APIUser apiUser)
        {
            APIUserDC.Delete(apiUser);
        }

        /// <summary>
        /// Looks up an APIUser by its api key and prepares it for use.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public static APIUser FindAndPrepare(string apiKey)
        {
            // Lookup user by api key.
            APIUser apiUser = APIUserDC.Get(apiKey);

            // Verify and prepare for use.
            if (apiUser != null && apiUser.PrepareForUse(apiKey))
            {
                // Return api user.
                return apiUser;
            }

            // Didn't find api user or preparation failed.
            return null;
        }
    }
}
