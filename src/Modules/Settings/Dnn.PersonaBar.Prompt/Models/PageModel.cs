namespace Dnn.PersonaBar.Prompt.Models
{
    public class PageModel : PageModelBase
    {
        public string Container;
        public string Url;
        public string Keywords;
        public string Description;

        public PageModel()
        {
        }
        public PageModel(DotNetNuke.Entities.Tabs.TabInfo tab): base(tab)
        {
            Container = tab.ContainerSrc;
            Url = tab.Url;
            Keywords = tab.KeyWords;
            Description = tab.Description;
        }
    }
}