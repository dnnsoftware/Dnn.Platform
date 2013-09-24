<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.ControlPanels.ControlBar"
    CodeFile="ControlBar.ascx.cs" %>
<%@ Import Namespace="DotNetNuke.Security.Permissions" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<!--SEO NOINDEX-->
<asp:Panel ID="ControlPanel" runat="server">
    <div id="ControlBar">
        <div class="ControlContainer">
            <div class="ServiceIcon professional">
                <asp:Image ID="conrolbar_logo" runat="server" ImageUrl="~/admin/controlpanel/controlbarimages/dnnLogo.png"
                    AlternateText="DNNlogo" />
                <%= GetUpgradeIndicator() %>
            </div>
            <!-- close ServiceIcon -->
            <ul id="ControlNav">
                <% if (UserController.GetCurrentUserInfo().IsInRole("Administrators"))
                   {%>
                <li><a href="<%= GetTabURL("Admin", false, Null.NullInteger) %>">
                    <%= GetString("Tool.Admin.Text") %></a>
                    <div class="subNav advanced">
                        <ul class="subNavToggle">
                            <li class="active BasicToggle"><a href="#controlbar_admin_basic" title="<%= GetString("Tool.CommonSettings.Text") %>">
                                <span></span></a></li>
                            <li class="AdvancedToggle"><a href="#controlbar_admin_advanced" title="<%= GetString("Tool.AdvancedSettings.Text") %>">
                                <span></span></a></li>
                            <li class="BookmarkToggle"><a href="#controlbar_admin_bookmarked" title="<%= GetString("Tool.BookmarkedSettings.Text") %>">
                                <span></span></a></li>
                        </ul>
                        <dl id="controlbar_admin_basic" class="active">
                            <dd>
                                <ul>
                                    <%= GetAdminBaseMenu() %>
                                </ul>
                            </dd>
                        </dl>
                        <dl id="controlbar_admin_advanced">
                            <dd>
                                <ul>
                                    <%= GetAdminAdvancedMenu() %>
                                </ul>
                            </dd>
                        </dl>
                        <dl id="controlbar_admin_bookmarked">
                            <dd>
                                <ul>
                                    <%= GetBookmarkItems("admin") %>
                                </ul>
                            </dd>
                        </dl>
                    </div>
                    <!--close subNav-->
                </li>
                <% if (UserController.GetCurrentUserInfo().IsSuperUser)
                   {%>
                <li><a href="<%= GetTabURL("Host", true, Null.NullInteger) %>">
                    <%= GetString("Tool.Host.Text") %></a>
                    <div class="subNav advanced">
                        <ul class="subNavToggle">
                            <li class="active BasicToggle"><a href="#controlbar_host_basic" title="<%= GetString("Tool.CommonSettings.Text") %>">
                                <span></span></a></li>
                            <li class="AdvancedToggle"><a href="#controlbar_host_advanced" title="<%= GetString("Tool.AdvancedSettings.Text") %>">
                                <span></span></a></li>
                            <li class="BookmarkToggle"><a href="#controlbar_host_bookmarked" title="<%= GetString("Tool.BookmarkedSettings.Text") %>">
                                <span></span></a></li>
                        </ul>
                        <dl id="controlbar_host_basic" class="active">
                            <dd>
                                <ul>
                                    <%= GetHostBaseMenu() %>
                                </ul>
                            </dd>
                        </dl>
                        <dl id="controlbar_host_advanced">
                            <dd>
                                <ul>
                                    <%= GetHostAdvancedMenu() %>
                                </ul>
                            </dd>
                        </dl>
                        <dl id="controlbar_host_bookmarked">
                            <dd>
                                <ul>
                                    <%= GetBookmarkItems("host") %>
                                </ul>
                            </dd>
                        </dl>
                    </div>
                    <!--close subNav-->
                </li>
                <% } %>
                <li class="controlBar_ArrowMenu"><a href="javascript:void(0)">
                    <%= GetString("Tool.Tools.Text") %></a>
                    <div class="subNav">
                        <dl>
                            <dd>
                                <ul>
                                    <li><a href='<%= BuildToolUrl("UploadFile", false, "File Manager", "Edit", "", true) %>'
                                        class="ControlBar_PopupLink">
                                        <%= GetString("Tool.UploadFile.Text") %></a></li>
                                   <% if (UserController.GetCurrentUserInfo().IsSuperUser)
                                      {%>
                                    <li><a href='javascript:void(0)' id="controlBar_ClearCache">
                                        <%= GetString("Tool.ClearCache.Text") %></a></li>
                                    <li><a href='javascript:void(0)' id="controlBar_RecycleAppPool">
                                        <%= GetString("Tool.RecycleApp.Text") %></a></li>
                                    <li>
                                        <div id="ControlBar_SiteSelector">
                                            <p>
                                                <%= GetString("Tool.SwitchSites.Text") %></p>
											<dnn:DnnComboBox runat="server" ID="controlBar_SwitchSite" ClientIDMode="Static" Skin="DnnBlack"/>
                                            <input type="submit" value="<%= GetString("Tool.SwitchSites.Button") %>" id="controlBar_SwitchSiteButton" class="dnnPrimaryAction" />
                                        </div>
                                    </li>
                                    <% if (ShowSwitchLanguagesPanel())
                                       { %>
                                    <li>
                                        <div id="ControlBar_LanguageSelector">
                                            <p>
                                               <%= GetString("Tool.SwitchLanguages.Text") %></p>
											<dnn:DnnComboBox runat="server" ID="controlBar_SwitchLanguage" ClientIDMode="Static" Skin="DnnBlack"/>
                                            <input type="submit" value="<%= GetString("Tool.SwitchSites.Button") %>" id="controlBar_SwitchLanguageButton" class="dnnPrimaryAction" />
                                        </div>
                                    </li>
                                    <% } %>
                                    <% } %>
                                </ul>
                            </dd>
                        </dl>
                    </div>
                    <!--close subNav-->
                </li>
                <li class="controlBar_ArrowMenu"><a href="javascript:void(0)">
                    <%= GetString("Tool.Help.Text") %></a>
                    <div class="subNav">
                        <dl>
                            <dd>
                                <ul>
                                    <asp:Literal ID="helpLink" runat="server"></asp:Literal>
                                    <li id="gettingStartedLink" runat="server"><a href="<%= GetTabURL("Getting Started", false) %>" class="ControlBar_PopupLink">
                                        <%= GetString("Tool.GettingStarted.Text") %></a></li>
                                </ul>
                            </dd>
                        </dl>
                    </div>
                    <!--close subNav-->
                </li>
                <% } %>
            </ul>
            <!--close ControlNav-->
            <ul id="ControlActionMenu">
               <% if (TabPermissionController.HasTabPermission("EDIT,CONTENT,MANAGE"))
                  {%>
                <li><a href="javascript:void(0)" id="ControlBar_Action_Menu">
                    <%= GetString("Tool.Modules.Text") %></a>
                    <ul>
                        <li><a href="javascript:void(0)" id="controlBar_AddNewModule">
                            <%= GetString("Tool.AddNewModule.Text") %></a> </li>
                        <li><a href="javascript:void(0)" id="controlBar_AddExistingModule">
                            <%= GetString("Tool.AddExistingModule.Text") %></a> </li>
                        <% if (UserController.GetCurrentUserInfo().IsSuperUser)
                           {%>
						<li class="separator"></li>
                        <li><a href="javascript:void(0)" id="controlBar_CreateModule">
                            <%= GetString("Tool.CreateModule.Text") %></a> </li>
                        <li><a href='<%= GetTabURL("Extensions", true) %>#moreExtensions'>
                            <%= GetString("Tool.FindModules.Text") %></a> </li>
                         <%   }%>
                   </ul>
                </li>
               <% } %>
               
                <% if (TabPermissionController.CanAddPage() || TabPermissionController.CanCopyPage() || TabPermissionController.CanImportPage())
                   {%>
                <li><a href="#">
                    <%= GetString("Tool.Pages.Text") %></a>
                    <ul>
                        <% if (TabPermissionController.CanAddPage() && CheckPageQuota())
                           {%>
                        <li><a href="<%= BuildToolUrl("NewPage", false, "", "", "", true) %>" class="ControlBar_PopupLink">
                            <%= GetString("Tool.AddNewPage.Text") %></a></li>
                        <% } %>
                        
                        <% if (TabPermissionController.CanCopyPage())
                           {%>
                        <li><a href="<%= BuildToolUrl("CopyPage", false, "", "", "", true) %>" class="ControlBar_PopupLink">
                            <%= GetString("Tool.CopyPage.Text") %></a></li>
                        <% } %>
                        
                        <% if (TabPermissionController.CanManagePage() && ActiveTabHasChildren() && !PortalSettings.ActiveTab.IsSuperTab)
                           {%>
                        <li><a href="javascript:void(0)" id="controlBar_CopyPermissionsToChildren">
                            <%= GetString("Tool.CopyPermissionsToChildren.Text") %></a></li>
                        <% } %>
                        
                        <% if (TabPermissionController.CanImportPage())
                           {%>
                        <li><a href="<%= BuildToolUrl("ImportPage", false, "", "", "", true) %>" class="ControlBar_PopupLink">
                            <%= GetString("Tool.ImportPage.Text") %></a></li>
                        <% } %>
                    </ul>
                </li>
                <% } %>
                
                 <% if (UserController.GetCurrentUserInfo().IsInRole("Administrators"))
                    {%>
                <li><a href="javascript:void(0)">
                    <%= GetString("Tool.Users.Text") %></a>
                    <ul>
                        <li>
                            <% if (PortalSettings.EnablePopUps)
                               {%>
                                <a href='<%= BuildToolUrl("NewUser", false, "User Accounts", "Edit", "", false) %>'
                                   class="ControlBar_PopupLink">
                            <% }
                               else
                               {%>
                                <a href='<%= BuildToolUrl("NewUser", false, "User Accounts", "Edit", "", false) %>'>
                               <%} %>
                            <%= GetString("Tool.NewUser.Text") %></a>
                        </li>
                        <li><a href='<%= GetTabURL("User Accounts", false) %>'>
                            <%= GetString("Tool.ManageUsers.Text") %></a></li>
                        <li><a href='<%= GetTabURL("Security Roles", false) %>'>
                            <%= GetString("Tool.ManageRoles.Text") %></a></li>
                    </ul>
                </li>
                <% } %>
            </ul>
           <% if (TabPermissionController.CanAddContentToPage() || TabPermissionController.CanManagePage() || TabPermissionController.CanAdminPage() ||
                  TabPermissionController.CanExportPage() || TabPermissionController.CanDeletePage()  || IsModuleAdmin())
           { %>
            <ul id="ControlEditPageMenu">
                <li><a href="javascript:void(0)" class="<%= SpecialClassWhenNotInViewMode() %>"><span
                    class="controlBar_editPageIcon"></span><span class="controlBar_editPageTxt">
                        <%= GetString("Tool.EditPage.Text") %></span></a>
                    <ul>
                        <% if (TabPermissionController.CanAddContentToPage() || IsModuleAdmin())
                           {%>
                        <li class="controlBar_BlueEditPageBtn"><a href="javascript:void(0)" id="ControlBar_EditPage">
                            <%= GetEditButtonLabel() %></a></li>
                      
                        <li>
                            <input type="checkbox" id="ControlBar_StayInEditMode" <%= CheckedWhenStayInEditMode() %> /><label
                                for="ControlBar_StayInEditMode"><%= GetString("Tool.StayInEditMode.Text") %></label></li>
                       
                        <li class="controlBar_EditPageSection">
                            <input type="checkbox" id="ControlBar_ViewInLayout" <%= CheckedWhenInLayoutMode() %> /><label
                                for="ControlBar_ViewInLayout"><%= GetString("Tool.LayoutMode.Text") %></label></li>
                      
                        <li><a href="javascript:void(0)" id="ControlBar_ViewInPreview">
                            <%= GetString("Tool.MobilePreview.Text") %></a></li>
                        <% } %>
                        <% if (TabPermissionController.CanManagePage())
                           {%>
                        <li><a href="<%= BuildToolUrl("PageSettings", false, "", "", "", true) %>" class="ControlBar_PopupLink">
                            <%= GetString("Tool.PageSettings.Text") %></a></li>
                        
                        <li><a href="<%= BuildToolUrl("PageTemplate", false, "", "", "", true) %>" class="ControlBar_PopupLink">
                            <%= GetString("Tool.ManageTemplate.Text") %></a></li>
                            <% if (PortalSettings.ContentLocalizationEnabled)
                               { %>
                        <li><a href="<%= BuildToolUrl("PageLocalization", false, "", "", "", true) %>" class="ControlBar_PopupLink">
                            <%= GetString("Tool.ManageLocalization.Text") %></a></li>
                            <% } %>
                        <% } %>
                        <% if (TabPermissionController.CanAdminPage())
                           {%>
                        <li><a href="<%= BuildToolUrl("PagePermission", false, "", "", "", true) %>" class="ControlBar_PopupLink">
                            <%= GetString("Tool.PagePermissions.Text") %></a></li>
                        <% } %>
                        <% if (TabPermissionController.CanExportPage())
                           {%>
                        <li><a href="<%= BuildToolUrl("ExportPage", false, "", "", "", true) %>" class="ControlBar_PopupLink">
                            <%= GetString("Tool.ExportPage.Text") %></a></li>
                        <% } %>
                        <% if (TabPermissionController.CanDeletePage())
                           {%>
                        <li><a href="<%= BuildToolUrl("DeletePage", false, "", "", "", true) %>" id="ControlBar_DeletePage">
                            <%= GetString("Tool.DeletePage.Text") %></a></li>
                        <% } %>
                       
                    </ul>
                    <div class="dnnClear">
                    </div>
                </li>
            </ul>
             <%}%>
        </div>
        
         <% if (TabPermissionController.HasTabPermission("EDIT,CONTENT,MANAGE"))
            {%>
        <div id="ControlBar_Module_AddNewModule" class="ControlModulePanel">
            <div class="ControlModuleContainer">
                <dnn:DnnComboBox ID="CategoryList" runat="server" DataTextField="Name" DataValueField="Name"  Skin="DnnBlack"
                    OnClientSelectedIndexChanged="dnn.controlBar.ControlBar_Module_CategoryList_Changed" />
                <a class="controlBar_CloseAddModules"><%= GetString("Cancel.Text") %></a>
            </div>
            <div id="ControlBar_ModuleListMessage_NewModule" class="ControlBar_ModuleListMessage">
                <p class="ControlBar_ModuleListMessage_NoResultMessage">
                    <%= GetString("NoModule.Text")%>
                </p>
            </div>
            <div id="ControlBar_ModuleListWaiter_NewModule" class="ControlBar_ModuleListWaiter">
                <p>
                    <%= GetString("LoadingModule.Text")%>
                </p>
            </div>
            <div id="ControlBar_ModuleListHolder_NewModule" class="ControlBar_ModuleListHolder">
                <ul class="ControlBar_ModuleList">
                </ul>
            </div>
            <div class="controlBar_ModuleListScrollDummy">
                <div class="controlBar_ModuleListScrollDummy_Content"></div>
            </div>
        </div>
        <div id="ControlBar_Module_AddExistingModule" class="ControlModulePanel">
            <div class="ControlModuleContainer">
                <dnn:DnnPageDropDownList ID="PageList" runat="server" CssClass="dnnLeftComboBox dnnBlackDropDown" IncludeAllTabTypes="True" IncludeDisabledTabs="True" />
                <dnn:DnnComboBox ID="VisibilityLst" runat="server" CssClass="dnnLeftComboBox" Enabled="false" Skin="DnnBlack" />
                <div class="ControlBar_chckCopyModule">
                    <input type="checkbox" id="ControlBar_Module_chkCopyModule" /><label for="ControlBar_Module_chkCopyModule"><%= GetString("Tool.MakeCopy.Text") %></label></div>
                <a class="controlBar_CloseAddModules"><%= GetString("Cancel.Text") %></a>
                <div class="dnnClear">
                </div>
            </div>
            <div id="ControlBar_ModuleListMessage_ExistingModule" class="ControlBar_ModuleListMessage">
                <p class="ControlBar_ModuleListMessage_InitialMessage">
                    <%= LoadTabModuleMessage %>
                </p>
                <p class="ControlBar_ModuleListMessage_NoResultMessage">
                    <%= GetString("NoModule.Text")%>
                </p>
            </div>
            <div id="ControlBar_ModuleListWaiter_ExistingModule" class="ControlBar_ModuleListWaiter">
                <p>
                    <%= GetString("LoadingModule.Text")%>
                </p>
            </div>
            <div id="ControlBar_ModuleListHolder_ExistingModule" class="ControlBar_ModuleListHolder">
                <ul class="ControlBar_ModuleList">
                </ul>
            </div>
            <div class="controlBar_ModuleListScrollDummy">
                <div class="controlBar_ModuleListScrollDummy_Content"></div>
            </div>
        </div>
        <ul id="ControlBar_Module_ModulePosition">
            <% var panes = LoadPaneList();%>
            <% foreach (var p in panes)
               { %>
            <li data-pane='<%= p[1] %>' data-position='<%= p[2] %>'>
                <%= p[0] %></li>
            <% } %>
            <div class="dnnClear">
            </div>
        </ul>
        <% } %>
    </div>
    <!--close ControlBar	-->
    <div id="shareableWarning">
        <h3>
            <%= GetString("ShareableWarningHeader") %></h3>
        <br />
        <p>
            <%= GetString("ShareableWarningContent") %>
        </p>
        <p>
            <%= GetString("ShareableWarningAlternate") %>
        </p>
        <br />
        <ul class="dnnActions dnnClear">
		    <li><a href="javascript:void(0)" id="shareableWarning_cmdConfirm" class="dnnPrimaryAction"><%= GetString("cmdConfirmAdd") %></a></li>
		    <li><a href="javascript:void(0)" id="shareableWarning_cmdCancel" class="dnnSecondaryAction"><%= GetString("cmdCancelAdd") %></a></li>
	    </ul>
    </div>
</asp:Panel>
<script type="text/javascript">
    if (typeof dnn === 'undefined') dnn = {};
    dnn.controlBarSettings = {
        currentUserMode: '<%= GetModeForAttribute() %>',
        categoryComboId: '<%= CategoryList.ClientID %>',
    	visibilityComboId: '<%= VisibilityLst.ClientID %>',
    	makeCopyCheckboxId: 'ControlBar_Module_chkCopyModule',
		pagePickerId: '<%= PageList.ClientID %>',
        yesText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(Localization.GetString("Yes.Text", Localization.SharedResourceFile)) %>',
        noText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(Localization.GetString("No.Text", Localization.SharedResourceFile)) %>',
        titleText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(Localization.GetString("Confirm.Text", Localization.SharedResourceFile)) %>',
        deleteText: '<%= GetButtonConfirmMessage("DeletePage") %>',
        copyPermissionsToChildrenText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(GetString("Tool.CopyPermissionsToChildrenPageEditor.Confirm")) %>',
            
        dragModuleToolTip: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(GetString("DragModuleToolTip.Text")) %>',
            
        loginUrl: '<%= LoginUrl %>',
		
        selectPageText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(GetString("SelectPage.Text")) %>',
        moduleShareableTitle: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(GetString("ShareableWarningTitle")) %>',		
        removeBookmarksTip: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(GetString("Tool.RemoveFromBookmarks.ToolTip")) %>'
    };
    
    $(function() {
        $('a#ControlBar_ViewInPreview').click(function() {
            <%=PreviewPopup() %>;
        });
    });

</script>
<!--END SEO-->