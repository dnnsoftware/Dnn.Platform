using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using DotNetNuke.Data;
using System.Collections.Generic;
using System.Linq;

namespace Cantarus.Modules.PolyDeploy.Components.DataAccess.DataControllers
{
    internal class APIUserDataController
    {
        #region Create

        public void Create(APIUser apiUser)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<APIUser>();

                repo.Insert(apiUser);
            }
        }

        #endregion

        #region Read

        public IEnumerable<APIUser> Get()
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<APIUser>();

                return repo.Get();
            }
        }

        public APIUser Get(int apiUserId)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<APIUser>();

                return repo.GetById<int>(apiUserId);
            }
        }

        public APIUser Get(string apiKey)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<APIUser>();

                return repo.Find("WHERE APIKey = @0", apiKey).FirstOrDefault<APIUser>();
            }
        }

        #endregion

        #region Update

        public void Update(APIUser apiUser)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<APIUser>();

                repo.Update(apiUser);
            }
        }

        #endregion

        #region Delete

        public void Delete(APIUser apiUser)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<APIUser>();

                repo.Delete(apiUser);
            }
        }

        #endregion
    }
}
