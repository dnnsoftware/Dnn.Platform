using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using DotNetNuke.Data;
using System.Linq;

namespace Cantarus.Modules.PolyDeploy.Components.DataAccess.DataControllers
{
    public class SessionDataController
    {
        public Session FindByGuid(string guid)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<Session>();

                return repo.Find("WHERE Guid = @0", guid).FirstOrDefault<Session>();
            }
        }

        public void Create(Session session)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<Session>();

                repo.Insert(session);
            }
        }

        public void Update(Session session)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<Session>();

                repo.Update(session);
            }
        }
    }
}
