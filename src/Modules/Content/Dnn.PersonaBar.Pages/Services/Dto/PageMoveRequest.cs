namespace Dnn.PersonaBar.Pages.Services.Dto
{
    public class PageMoveRequest
    {
        public int PageId { get; set; }

        public int RelatedPageId { get; set; }

        public int ParentId { get; set; }

        public string Action { get; set; }
    }
}