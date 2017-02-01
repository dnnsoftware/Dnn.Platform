using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class PageInfoModel
    {
        // Order of properties is important for client-side display. Declare most important/useful properties first.
        public int TabId;
        public string __TabId;          // command link
        public string Name;
        public string Title;
        public int ParentId;
        public string __ParentId;       // command link
        public string Container;
        public string Skin;
        public string Path;
        public bool IncludeInMenu;
        public string __IncludeInMenu;  // command link
        public bool IsDeleted;
        public string __IsDeleted;      // command link
        public string Url;
        public string Keywords;

        public string Description;
        public static PageInfoModel FromDnnTabInfo(DotNetNuke.Entities.Tabs.TabInfo tab)
        {
            PageInfoModel page = new PageInfoModel()
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