using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dnn.EditBar.Library.Items;

namespace Dnn.EditBar.UI.Controllers
{
    public interface IEditBarController
    {
        IDictionary<string, object> GetConfigurations(int portalId);

        IList<BaseMenuItem> GetMenuItems();
    }
}
