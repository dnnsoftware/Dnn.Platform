using System.Collections.Generic;

namespace DotNetNuke.Services.Registration
{
    public interface IRegistrationProfileController
    {
        IEnumerable<string> Search(int portalId, string searchValue);
    }
}