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
    public static class OAUTHDataController
    {
        public static Client ClientRepositoryGetById(string id)
        {

            return CBO.FillObject<Client>(DataProvider.Instance().ExecuteReader("ClientRepositoryGetById", id));
            
        }

        public static OAUTHUser UserRepositoryGetById(string id)
        {

            return CBO.FillObject<OAUTHUser>(DataProvider.Instance().ExecuteReader("UserRepositoryGetById", id));
            
        }

        public static void OAuthUserInsert(OAUTHUser user)
        {

            DataProvider.Instance().ExecuteNonQuery("OAuthUserInsert", user.Id,user.CreateDateUtc);

        }

        public static void ClientInsert(string clientId,string clientSecret,string callback,string name,int clientType)
        {

            DataProvider.Instance().ExecuteNonQuery("ClientInsert", clientId,clientSecret,callback,name,clientType);

        }

        public static void DeleteClient(string clientId)
        {

            DataProvider.Instance().ExecuteNonQuery("ClientDelete", clientId);

        }
        

        public static void OAuthAuthorizationInsert(Authorization auth)
        {

            DataProvider.Instance().ExecuteNonQuery("OAuthAuthorizationInsert", auth.ClientId,auth.UserId,auth.ResourceId,auth.Scope,auth.CreatedOnUtc,auth.ExpirationDateUtc);

        }
        

        public static Resource ResourceRepositoryFindWithSupportedScopes(HashSet<string> scopes)
        {
            //stub scopes as not using
            return CBO.FillObject<Resource>(DataProvider.Instance().ExecuteReader("ResourceRepositoryFindWithSupportedScopes", ""));
            
        }

        public static IEnumerable<Authorization> AuthorizationRepositoryFindCurrent(string clientIdentifier, string userIdentifier, DateTime afterUtc)
        {
            //stub scopes as not using
            return CBO.FillCollection<Authorization>(DataProvider.Instance().ExecuteReader("AuthorizationRepositoryFindCurrent", clientIdentifier, userIdentifier, afterUtc));
            
        }
        

        
    }
}
