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

        public IPSpec FindByAddress(string address)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<IPSpec>();

                return repo.Find("WHERE Address = @0", address).FirstOrDefault<IPSpec>();
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
