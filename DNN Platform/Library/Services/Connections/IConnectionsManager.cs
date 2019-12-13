using System.Collections.Generic;

namespace DotNetNuke.Services.Connections
{
    public interface IConnectionsManager
    {
        void RegisterConnections();

        IList<IConnector> GetConnectors();
    }
}
