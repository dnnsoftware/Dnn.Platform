using Dnn.PersonaBar.Users.Components.Contracts;
using Dnn.PersonaBar.Users.Components.Dto;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components
{
    internal interface IRegisterController
    {
        UserBasicDto Register(RegisterationDetails registerationDetails);
    }
}
