using Dnn.PersonaBar.Pages.Services.Dto;

namespace Dnn.PersonaBar.Pages.Components
{
    public interface IBulkPagesController
    {
        BulkPageResponse AddBulkPages(BulkPage page, bool validateOnly);
    }
}