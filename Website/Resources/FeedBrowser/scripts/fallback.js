
// Fallback for Medium Trust when the main OPML file cannot be retrieved.
function defaultFeedBrowser()
{

    var tabs = [];     
    with (DotNetNuke.UI.WebControls.FeedBrowser)      
    {
        var tab2 = new TabInfo('Marketplace','http://marketplace.dotnetnuke.com/rssfeed.aspx?channel=marketplacesolutions&affiliateid=10054','marketplace');
        var tab2_0 = tab2.addSection(new SectionInfo('Modules', 'http://marketplace.dotnetnuke.com/feed-sp-2-10054.aspx'));
        tab2_0.addCategory(new CategoryInfo('Community Management','http://marketplace.dotnetnuke.com/feed-sc-2-18-10054.aspx',0));
        tab2_0.addCategory(new CategoryInfo('Discussion/Forum','http://marketplace.dotnetnuke.com/feed-sc-2-19-10054.aspx',1));
        tab2_0.addCategory(new CategoryInfo('Content Management','http://marketplace.dotnetnuke.com/feed-sc-2-2-10054.aspx',0));
        tab2_0.addCategory(new CategoryInfo('Data Management','http://marketplace.dotnetnuke.com/feed-sc-2-11-10054.aspx',1));
        tab2_0.addCategory(new CategoryInfo('Document Management','http://marketplace.dotnetnuke.com/feed-sc-2-3-10054.aspx',1));
        tab2_0.addCategory(new CategoryInfo('File Management','http://marketplace.dotnetnuke.com/feed-sc-2-6-10054.aspx',1));
        tab2_0.addCategory(new CategoryInfo('eCommerce','http://marketplace.dotnetnuke.com/feed-sc-2-4-10054.aspx',0));
        tab2_0.addCategory(new CategoryInfo('Media Management','http://marketplace.dotnetnuke.com/feed-sc-2-5-10054.aspx',0));
        tab2_0.setDefaultCategory(7);
        tab2_0.addCategory(new CategoryInfo('Image Viewer','http://marketplace.dotnetnuke.com/feed-sc-2-9-10054.aspx',1));
        tab2_0.addCategory(new CategoryInfo('Photo Albums','http://marketplace.dotnetnuke.com/feed-sc-2-7-10054.aspx',1));
        tab2_0.addCategory(new CategoryInfo('Multi-Language','http://marketplace.dotnetnuke.com/feed-sc-2-14-10054.aspx',0));
        tab2_0.addCategory(new CategoryInfo('Navigation','http://marketplace.dotnetnuke.com/feed-sc-2-10-10054.aspx',0));
        tab2_0.addCategory(new CategoryInfo('Skinning/CSS Management','http://marketplace.dotnetnuke.com/feed-sc-2-16-10054.aspx',0));
        tab2_0.addCategory(new CategoryInfo('Time Management','http://marketplace.dotnetnuke.com/feed-sc-2-17-10054.aspx',0));
        tab2_0.addCategory(new CategoryInfo('Project Management','http://marketplace.dotnetnuke.com/feed-sc-2-8-10054.aspx',1));
        var tab2_1 = tab2.addSection(new SectionInfo('Skins', 'http://marketplace.dotnetnuke.com/feed-sp-3-10054.aspx'));
        tab2_1.addCategory(new CategoryInfo('Skins','http://marketplace.dotnetnuke.com/feed-sc-3-20-10054.aspx',0));
        tab2_1.setDefaultCategory(0);
        var tab2_2 = tab2.addSection(new SectionInfo('Other', 'http://marketplace.dotnetnuke.com/feed-sp-8-10054.aspx'));
        tabs[tabs.length] = tab2;
        var tab3 = new TabInfo('DotNetNuke','','dotnetnuke');
        var tab3_0 = tab3.addSection(new SectionInfo('Projects', 'http://www.dotnetnuke.com/'));
        tab3_0.addCategory(new CategoryInfo('Modules','http://www.dotnetnuke.com/portals/25/SolutionsExplorer/Projects-Modules.xml',0));
        tab3_0.addCategory(new CategoryInfo('Providers','http://www.dotnetnuke.com/portals/25/SolutionsExplorer/Projects-Providers.xml',0));
        tab3_0.addCategory(new CategoryInfo('Utility','http://www.dotnetnuke.com/portals/25/SolutionsExplorer/Projects-Utility.xml',0));
        tab3_0.addCategory(new CategoryInfo('Core','http://www.dotnetnuke.com/portals/25/SolutionsExplorer/Projects-Core.xml',0));
        tab3_0.addCategory(new CategoryInfo('Component','http://www.dotnetnuke.com/portals/25/SolutionsExplorer/Projects-Component.xml',0));
        tabs[tabs.length] = tab3;
        var tab4 = new TabInfo('About','');
        var tab4_0 = tab4.addSection(new SectionInfo('Solutions Explorer', 'http://www.dotnetnuke.com/portals/25/SolutionsExplorer/SolutionsExplorerInfo.xml'));
        var tab4_1 = tab4.addSection(new SectionInfo('DotNetNuke Marketplace', 'http://www.dotnetnuke.com/portals/25/SolutionsExplorer/MarketplaceInfo.xml'));
        var tab4_2 = tab4.addSection(new SectionInfo('Coming Soon', 'http://www.dotnetnuke.com/portals/25/SolutionsExplorer/ComingSoon.xml'));
        tabs[tabs.length] = tab4;
      }
     return(tabs);
}
