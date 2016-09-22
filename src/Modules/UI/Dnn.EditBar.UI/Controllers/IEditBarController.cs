using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnn.EditBar.UI.Controllers
{
    public interface IEditBarController
    {
        IDictionary<string, object> GetConfigurations(int portalId);
    }
}
