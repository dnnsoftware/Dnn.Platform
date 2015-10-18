using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using OAuth.AuthorizationServer.Core.Data.Model;


namespace OAuth.AuthorizationServer.Core.Server
{
    /// <summary>
    /// class to integrate with DNN's data store
    /// </summary>
    public static class OAUTHDataController
    {

        /// <summary>
        /// Get the OAUTH settings
        /// </summary>
        /// <returns></returns>
        public static Settings GetSettings()
        {

            return CBO.FillObject<Settings>(DataProvider.Instance().ExecuteReader("GetOAuthSettings"));

        }

        /// <summary>
        /// Insert oauth settings
        /// </summary>
        /// <param name="authorizationServerPrivateKey"></param>
        /// <param name="resourceServerDecryptionKey"></param>
        /// <param name="authorizationServerVerificationKey"></param>
        public static void InsertSettings(string authorizationServerPrivateKey, string resourceServerDecryptionKey, string authorizationServerVerificationKey)
        {

            DataProvider.Instance().ExecuteNonQuery("InsertOAuthSettings",authorizationServerPrivateKey,resourceServerDecryptionKey,authorizationServerVerificationKey);

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Client ClientRepositoryGetById(string id)
        {

            return CBO.FillObject<Client>(DataProvider.Instance().ExecuteReader("ClientRepositoryGetById", id));
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static OAUTHUser UserRepositoryGetById(string id)
        {

            return CBO.FillObject<OAUTHUser>(DataProvider.Instance().ExecuteReader("UserRepositoryGetById", id));
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        public static void OAuthUserInsert(OAUTHUser user)
        {

            DataProvider.Instance().ExecuteNonQuery("OAuthUserInsert", user.Id,user.CreateDateUtc);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="callback"></param>
        /// <param name="name"></param>
        /// <param name="clientType"></param>
        public static void ClientInsert(string clientId,string clientSecret,string callback,string name,int clientType)
        {

            DataProvider.Instance().ExecuteNonQuery("ClientInsert", clientId,clientSecret,callback,name,clientType);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        public static void DeleteClient(string clientId)
        {

            DataProvider.Instance().ExecuteNonQuery("ClientDelete", clientId);

        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="auth"></param>
        public static void OAuthAuthorizationInsert(Authorization auth)
        {

            DataProvider.Instance().ExecuteNonQuery("OAuthAuthorizationInsert", auth.ClientId,auth.UserId,auth.ResourceId,auth.Scope,auth.CreatedOnUtc,auth.ExpirationDateUtc);

        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scopes"></param>
        /// <returns></returns>
        public static Resource ResourceRepositoryFindWithSupportedScopes(HashSet<string> scopes)
        {
            //stub scopes as not using
            return CBO.FillObject<Resource>(DataProvider.Instance().ExecuteReader("ResourceRepositoryFindWithSupportedScopes", ""));
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientIdentifier"></param>
        /// <param name="userIdentifier"></param>
        /// <param name="afterUtc"></param>
        /// <returns></returns>
        public static IEnumerable<Authorization> AuthorizationRepositoryFindCurrent(string clientIdentifier, string userIdentifier, DateTime afterUtc)
        {
            //stub scopes as not using
            return CBO.FillCollection<Authorization>(DataProvider.Instance().ExecuteReader("AuthorizationRepositoryFindCurrent", clientIdentifier, userIdentifier, afterUtc));
            
        }
        

        
    }
}
