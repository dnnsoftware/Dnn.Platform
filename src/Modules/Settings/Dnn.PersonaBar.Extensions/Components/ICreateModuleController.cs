using Dnn.PersonaBar.Extensions.Components.Dto;

namespace Dnn.PersonaBar.Extensions.Components
{
    public interface ICreateModuleController
    {
        bool CreateModule(CreateModuleDto createModuleDto, out string newPageUrl, out string errorMessage);
    }
}