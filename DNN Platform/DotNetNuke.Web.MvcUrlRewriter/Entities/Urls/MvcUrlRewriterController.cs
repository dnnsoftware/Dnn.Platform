using System.Collections.Specialized;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using System.Web;
using DotNetNuke.Abstractions.Portals;

namespace DotNetNuke.Web.MvcUrlRewriter.Entities.Urls
{
    internal class MvcUrlRewriterController
    {
        internal static bool IsMvc(UrlAction result, NameValueCollection queryStringCol, HttpContext context, int tabId, int portalId)
        {
            var mvcCtls = new[] { /*"Module",*/ "Terms", "Privacy" };
            bool mvcCtl = false;
            /*
            bool mvcSkin = false;
            if (context.Items.Contains("PortalSettings"))
            {
                var ps = (PortalSettings)context.Items["PortalSettings"];
                if (ps != null)
                {
                    mvcSkin = !string.IsNullOrEmpty(PortalSettings.Current.ActiveTab.SkinSrc) &&
                            PortalSettings.Current.ActiveTab.SkinSrc.EndsWith("mvc");
                }
            }
            */

            if (result.RewritePath.Contains("&ctl="))
            {
                foreach (var item in mvcCtls)
                {
                    mvcCtl = mvcCtl || result.RewritePath.Contains("&ctl=" + item);
                }
                /*
                if (mvcCtl && result.RewritePath.Contains("&ctl=Module"))
                {
                    TabInfo tab = null;
                    if (tabId > 0 && portalId > -1)
                    {
                        tab = TabController.Instance.GetTab(tabId, portalId, false);
                        if (tab != null)
                        {
                            mvcCtl = tab.GetTags().Contains("mvc");
                        }
                    }

                    // mvcCtl = queryStringCol["ReturnURL"] != null && queryStringCol["ReturnURL"].EndsWith("mvc");
                }
                */
            }
            else
            {
                TabInfo tab = null;
                if (tabId > 0 && portalId > -1)
                {
                    tab = TabController.Instance.GetTab(tabId, portalId, false);
                    if (tab != null)
                    {
                        // mvcCtl = tab.GetTags().Contains("mvc");
                        var tabPipeline = tab.PagePipeline;
                        if (!string.IsNullOrEmpty(tabPipeline))
                        {
                            mvcCtl = tabPipeline == PagePipelineConstants.Mvc;
                        }
                        else
                        {

                            var portalPipeline = PortalSettingsController.Instance().GetPortalPagePipeline(portalId);
                            mvcCtl = portalPipeline == PagePipelineConstants.Mvc;
                        }
                    }
                }
            }

            mvcCtl = mvcCtl && !result.RewritePath.Contains("mvcpage=no") && queryStringCol["mvcpage"] != "no";
            mvcCtl = mvcCtl || result.RewritePath.Contains("mvcpage=yes") || queryStringCol["mvcpage"] == "yes";
            return mvcCtl;
        }
    }
}
