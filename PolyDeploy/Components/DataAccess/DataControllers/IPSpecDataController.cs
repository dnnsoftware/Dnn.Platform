using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using DotNetNuke.Data;
using System.Collections.Generic;
using System.Linq;

namespace Cantarus.Modules.PolyDeploy.Components.DataAccess.DataControllers
{
    internal class IPSpecDataController
    {
        #region Create

        public void Create(IPSpec ipSpec)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<IPSpec>();

                repo.Insert(ipSpec);
            }
        }

        #endregion

        #region Read

        public IEnumerable<IPSpec> Get()
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<IPSpec>();

                return repo.Get();
            }
        }

        public IPSpec Get(int ipSpecId)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<IPSpec>();

                return repo.GetById<int>(ipSpecId);
            }
        }

        public IPSpec Get(string address)
        {
            using (IDataContext context = DataContext.Instance())
            {
                return context.ExecuteSingleOrDefault<IPSpec>(
                    System.Data.CommandType.StoredProcedure,
                    "{databaseOwner}[{objectQualifier}Cantarus_PolyDeploy_IPSpecByAddress]",
                    address
                );
            }
        }

        public IPSpec GetByName(string name)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<IPSpec>();

                return repo.Find("WHERE [Name] = @0", name).FirstOrDefault<IPSpec>();
            }
        }

        #endregion

        #region Update

        // N/A

        #endregion

        #region Delete

        public void Delete(IPSpec ipSpec)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<IPSpec>();

                repo.Delete(ipSpec);
            }
        }

        #endregion
    }
}
