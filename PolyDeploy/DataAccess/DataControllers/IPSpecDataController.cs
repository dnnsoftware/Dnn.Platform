using Cantarus.Modules.PolyDeploy.DataAccess.Models;
using DotNetNuke.Data;
using System.Linq;

namespace Cantarus.Modules.PolyDeploy.DataAccess.DataControllers
{
    public class IPSpecDataController
    {
        public IPSpec FindByAddress(string address)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<IPSpec>();

                return repo.Find("WHERE Address = @0", address).FirstOrDefault<IPSpec>();
            }
        }

        public void Create(IPSpec ipSpec)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<IPSpec>();

                repo.Insert(ipSpec);
            }
        }
    }
}
