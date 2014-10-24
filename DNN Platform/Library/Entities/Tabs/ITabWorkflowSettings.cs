using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetNuke.Entities.Tabs
{
    public interface ITabWorkflowSettings
    {
        bool IsWorkflowEnabled(int portalId, int tabId);
    }
}
