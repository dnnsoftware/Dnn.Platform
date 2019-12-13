using DotNetNuke.Framework.JavaScriptLibraries;

namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    public partial class jQuery : SkinObjectBase
    {
        public bool DnnjQueryPlugins { get; set; }
        public bool jQueryHoverIntent { get; set; }
        public bool jQueryUI { get; set; }

        protected override void OnInit(EventArgs e)
        {
            JavaScript.RequestRegistration(CommonJs.jQuery);
            JavaScript.RequestRegistration(CommonJs.jQueryMigrate);

            if (jQueryUI)
            {
                JavaScript.RequestRegistration(CommonJs.jQueryUI);
            }

            if (DnnjQueryPlugins)
            {
                JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            }

            if (jQueryHoverIntent)
            {
                JavaScript.RequestRegistration(CommonJs.HoverIntent);
            }
        }
    }
}
