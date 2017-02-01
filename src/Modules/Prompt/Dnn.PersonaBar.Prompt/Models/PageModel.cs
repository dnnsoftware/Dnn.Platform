using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class PageModel:PageModelBase
    {
        // Order of properties is important for client-side display. Declare most important/useful properties first.
        public string Container;
        public string Url;
        public string Keywords;

        public string Description;
        public static new PageModel FromDnnTabInfo(DotNetNuke.Entities.Tabs.TabInfo tab)
        {
            PageModel page = new PageModel()
            {
                Name = tab.TabName,
                ParentId = tab.ParentId,
                Path = tab.TabPath,
                TabId = tab.TabID,
                Skin = tab.SkinSrc,
                Title = tab.Title,
                IncludeInMenu = tab.IsVisible,
                IsDeleted = tab.IsDeleted,
                Url = tab.Url,
                Keywords = tab.KeyWords,
                Description = tab.Description
            };

            page.__ParentId = string.Format("list-pages --parentid {0}", page.ParentId);
            page.__TabId = string.Format("goto {0}", page.TabId);
            page.__IncludeInMenu = string.Format("list-pages --visible{0}", (page.IncludeInMenu ? "" : " false"));
            page.__IsDeleted = string.Format("list-pages --deleted{0}", (page.IsDeleted ? "" : " false"));

            return page;
        }
    }
}