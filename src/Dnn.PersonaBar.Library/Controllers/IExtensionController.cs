using System.Collections.Generic;
using Dnn.PersonaBar.Library.Model;

namespace Dnn.PersonaBar.Library.Controllers
{
    public interface IExtensionController
    {
        string GetPath(PersonaBarExtension extension);

        bool Visible(PersonaBarExtension extension);

        IDictionary<string, object> GetSettings(PersonaBarExtension extension);
    }
}
