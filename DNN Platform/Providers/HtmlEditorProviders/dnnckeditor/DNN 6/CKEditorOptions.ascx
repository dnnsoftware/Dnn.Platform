<%@ Control Language="c#" AutoEventWireup="True" Codebehind="CKEditorOptions.ascx.cs" Inherits="WatchersNET.CKEditor.CKEditorOptions" %>
<%@ Register TagPrefix="dnn" TagName="URL" Src="UrlControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls"  %>

<div id="SettingsBox">
  <h1>CKEditor Provider <asp:Label id="lblSettings" runat="server">Settings</asp:Label></h1>
  <hr />
  <table style="width:100%;">
    <tr>
      <td>
        <asp:Label id="lblSetFor" runat="server">Modify Settings for...</asp:Label>
      </td>
      <td>
        <asp:RadioButtonList id="rBlSetMode" runat="server" RepeatDirection="Horizontal" AutoPostBack="true">
          <asp:ListItem Text="Portal" Value="portal" Selected="True"></asp:ListItem>
          <asp:ListItem Text="Page" Value="page"></asp:ListItem>
          <asp:ListItem Text="Module Instance" Value="minstance"></asp:ListItem>
        </asp:RadioButtonList>
      </td>
    </tr>
    <tr>
        <td></td>
        <td>
            <div class="Toolbar">
                <asp:LinkButton id="CopyToAllChild" runat="server" Text="Copy Settings to Child Pages" CssClass="copyButton"></asp:LinkButton>
            </div>
            <div class="Toolbar">
                <asp:LinkButton id="lnkRemoveAll" runat="server" Text="Delete All Settings" CssClass="removeButton"></asp:LinkButton>
                <asp:LinkButton id="lnkRemoveChild" runat="server" Text="Delete Child Settings" CssClass="removeButton"></asp:LinkButton>
                <asp:LinkButton id="lnkRemove" runat="server" Text="Delete Settings" CssClass="removeButton"></asp:LinkButton>
            </div>
            <div class="Toolbar">
                <a onclick="showDialog('ImportDialog');" id="ckeditoroptions_lnkImport" class="importButton" href="#"><asp:Label id="lblImport" runat="server" Text="Import"></asp:Label></a>
                <a onclick="showDialog('ExportDialog');" id="ckeditoroptions_Export" href="#" class="exportButton"><asp:Label id="lblExport" runat="server" Text="Export"></asp:Label></a>
            </div>
      </td>
    </tr>
  </table>
  <div id="ExportDialog" title='<%= DotNetNuke.Services.Localization.Localization.GetString("SettingsExportTitle.Text", this.ResXFile, this.LangCode) %>' style="display:none">
  <asp:UpdatePanel ID="ExportDialogUpdatePanel" UpdateMode="Conditional" ChildrenAsTriggers="true" runat="server">
            <ContentTemplate>
                <div><asp:DropDownList id="ExportDir" runat="server" Width="300"></asp:DropDownList></div>
                <div style="margin-top:6px"><asp:TextBox id="ExportFileName" runat="server" Width="294"></asp:TextBox></div>
                <asp:LinkButton id="ExportNow" runat="server" OnClick="Export_Click" Text="Export Now" Visible="true" CssClass="Hidden ExportHidden"></asp:LinkButton>
                <asp:HiddenField id="HiddenMessage" runat="server" Value=""/>
            </ContentTemplate>
            </asp:UpdatePanel>
  </div>
  <div id="ImportDialog" title='<%= DotNetNuke.Services.Localization.Localization.GetString("SettingsImportTitle.Text", this.ResXFile, this.LangCode) %>' style="display:none">
  <asp:UpdatePanel ID="upNewUpdatePanel" UpdateMode="Conditional" ChildrenAsTriggers="true" runat="server">
            <ContentTemplate>
                <dnn:url id="ctlImportFile" runat="server" width="300" showtabs="False" Required="False" filefilter="xml" showupload="False" showfiles="True" showUrls="False"
					urltype="F" showlog="False" shownewwindow="False" showtrack="False"></dnn:url>
    <asp:LinkButton id="lnkImportNow" runat="server" OnClick="Import_Click" Text="Import Now" Visible="true" CssClass="Hidden ImportHidden"></asp:LinkButton>
            </ContentTemplate>
            </asp:UpdatePanel>
  </div>
  
  <asp:UpdatePanel ID="upOptions" UpdateMode="Conditional" runat="server">
        <ContentTemplate>
  <table style="width:100%;">
    <tr>
      <td>
        <asp:HiddenField id="LastTabId" runat="server" Value="1"/>
        <div id="SettingsTabs" class="SettingBox">
          <ul>
            <li runat="server" id="InfoTabLi"><a href="#InfoTab"><asp:Label id="lblHeader" runat="server">Informations</asp:Label></a></li>
            <li><a href="#MainSetTab"><asp:label id="lblMainSet" runat="server">Main Settings</asp:label></a></li>
            <li><a href="#EditorConfigSetTab"><asp:label id="lblEditorConfig" runat="server">Editor Config</asp:label></a></li>
            <li><a href="#BrowserSetTab"><asp:label id="lblBrowsSec" runat="server">File Browser Settings</asp:label></a></li>
            <li><a href="#ToolbarsSetTab"><asp:label id="lblCustomToolbars" runat="server">Custom Toolbars</asp:label></a></li>
          </ul>
          <!-- BEGIN Info Tab -->
           <asp:PlaceHolder runat="server" ID="InfoTabHolder">
          <div id="InfoTab">
            <ul>
              <li><asp:Label id="ProviderVersion" runat="server">Editor Provider Version:</asp:Label></li>
              <li><asp:Label id="lblPortal" runat="server">Portal:</asp:Label></li>
              <li><asp:Label id="lblPage" runat="server">Page:</asp:Label></li>
              <li><asp:Label id="lblModType" runat="server">Module type:</asp:Label></li>
              <li><asp:Label id="lblModName" runat="server">Module Name:</asp:Label></li>
              <li><asp:Label id="lblModInst" runat="server">Module Instance:</asp:Label></li>
              <li><asp:Label id="lblUName" runat="server">User Name:</asp:Label></li>
            </ul>
          </div>
          </asp:PlaceHolder>
          <!-- END Info Tab -->

          <!-- BEGIN Main Settings Tab -->
    <div id="MainSetTab">
    <table id="tblMainSet">
      <tr>
	    <td class="settingNameColumn"><asp:label id="lblBlanktext" runat="server">Blank Text:</asp:label></td>
	    <td class="settingValueColumn"><dnn:DnnTextBox runat="server" id="txtBlanktext" Width="395px" /></td>
      </tr>
      <tr>
	    <td class="settingNameColumn"><asp:label id="lblSkin" runat="server">Skin:</asp:label></td>
	    <td class="settingValueColumn"><asp:dropdownlist id="ddlSkin" runat="server" CssClass="DefaultDropDown"></asp:dropdownlist></td>
      </tr>
      <tr>
	    <td class="settingNameColumn"><asp:label id="CodeMirrorLabel" runat="server">CodeMirror Theme (for Source Syntax Highlighting):</asp:label></td>
	    <td class="settingValueColumn"><asp:dropdownlist id="CodeMirrorTheme" runat="server" CssClass="DefaultDropDown"></asp:dropdownlist></td>
      </tr>
      <tr>
	    <td class="settingNameColumn"><asp:label id="lblToolbars" runat="server">Toolbars:</asp:label></td>
	    <td class="settingValueColumn">
	      <asp:GridView id="gvToolbars" runat="server" AutoGenerateColumns="False" Width="400px" GridLines="None" HeaderStyle-HorizontalAlign="Left" HeaderStyle-Font-Bold="false" HeaderStyle-Font-Italic="true">
            <Columns>
              <asp:TemplateField>
                <HeaderStyle HorizontalAlign="Left"></HeaderStyle>
                <HeaderTemplate>
                  <asp:Label id="lblRole" runat="server"></asp:Label>
                </HeaderTemplate>
                <ItemStyle Width="200"></ItemStyle>
                <ItemTemplate>
                  <asp:Label ID="lblRoleName" runat="server" Text="<%# Container.DataItem.ToString()%>"></asp:Label>
                </ItemTemplate>
              </asp:TemplateField>
              <asp:TemplateField>
                <HeaderStyle HorizontalAlign="Left"></HeaderStyle>                  
                <HeaderTemplate>
                  <asp:Label id="lblSelToolb" runat="server"></asp:Label>
                </HeaderTemplate>
                <ItemTemplate>
                  <asp:dropdownlist id="ddlToolbars" runat="server" Width="200">
	              </asp:dropdownlist>
                </ItemTemplate>
              </asp:TemplateField>
            </Columns>
          </asp:GridView>
        </td>
      </tr>
       <tr>
        <td class="settingNameColumn"><asp:label id="lblCustomConfig" runat="server">Custom Config File</asp:label></td>
        <td class="settingValueColumn"><dnn:url id="ctlConfigUrl" runat="server" width="400" filefilter="js"></dnn:url></td>
      </tr>
      <tr>
        <td class="settingNameColumn">
          <asp:label id="lblWidth" runat="server">Editor Width:</asp:label>
        </td>
        <td class="settingValueColumn">
          <asp:TextBox runat="server" id="txtWidth" Width="395px" CssClass="settingValueInputNumeric" />
        </td>
      </tr>
      <tr>
        <td class="settingNameColumn">
          <asp:label id="lblHeight" runat="server">Editor Height:</asp:label>
        </td>
        <td class="settingValueColumn">
          <asp:TextBox runat="server" id="txtHeight" Width="395px" CssClass="settingValueInputNumeric" />
        </td>
      </tr>
      <tr>
        <td class="settingNameColumn"><asp:label id="lblInjectSyntaxJs" runat="server">Inject Syntax Highlighter Js Code?</asp:label></td>
        <td class="settingValueColumn"><asp:CheckBox ID="InjectSyntaxJs" runat="server" Checked="true"></asp:CheckBox></td>
      </tr>
      <tr>
           <td class="settingNameColumn">        
                    <asp:label id="lblCssurl" runat="server">Editor area CSS</asp:label>
           </td>
           <td class="settingValueColumn">
                    <dnn:url id="ctlCssurl" runat="server" width="400" showtabs="False" Required="False" filefilter="css" showupload="False" showfiles="True" showUrls="True"
					urltype="F" showlog="False" shownewwindow="False" showtrack="False"></dnn:url>
            </td>

      </tr>
      <tr>
        <td class="settingNameColumn"><asp:label id="lblTemplFiles" runat="server">Editor Templates File</asp:label></td>
        <td class="settingValueColumn">
                <dnn:url id="ctlTemplUrl" runat="server" width="400" showtabs="False" Required="False" filefilter="xml,js" showupload="False" showfiles="True" showUrls="True"
					urltype="F" showlog="False" shownewwindow="False" showtrack="False"></dnn:url>
        </td>
      </tr>
      <tr>
        <td class="settingNameColumn"><asp:label id="CustomJsFileLabel" runat="server">Custom JS File</asp:label></td>
        <td class="settingValueColumn">
                <dnn:url id="ctlCustomJsFile" runat="server" width="400" showtabs="False" Required="False" filefilter="js" showupload="False" showfiles="True" showUrls="True"
					urltype="F" showlog="False" shownewwindow="False" showtrack="False"></dnn:url>
        </td>
      </tr>
	</table>
  
        </div>
        <!-- END Main Settings Tab -->
            

        <div id="BrowserSetTab">



    <table>
    <tr>
	    <td class="settingNameColumn"><asp:label id="lblBrowser" runat="server">File Browser:</asp:label></td>
	    <td class="settingValueColumn">
	      <asp:dropdownlist id="ddlBrowser" runat="server" CssClass="DefaultDropDown">
	        <asp:ListItem Text="None" Value="none" Enabled="true"></asp:ListItem>
	        <asp:ListItem Text="Standard" Value="standard" ></asp:ListItem>
	        <asp:ListItem Text="CKFinder" Value="ckfinder"></asp:ListItem>
	      </asp:dropdownlist></td>
      </tr>
      <tr>
        <td class="settingNameColumn"><asp:label id="lblBrowAllow" runat="server">File Browser Security</asp:label></td>
        <td class="settingValueColumn"><asp:CheckBoxList ID="chblBrowsGr" runat="server"></asp:CheckBoxList></td>
      </tr>
      <tr>
        <td class="settingNameColumn"><asp:label id="BrowserRootFolder" runat="server">Browser Root Folder</asp:label></td>
        <td class="settingValueColumn"><asp:DropDownList ID="BrowserRootDir" runat="server" CssClass="DefaultDropDown"></asp:DropDownList></td>
      </tr>
      <tr>
        <td class="settingNameColumn"><asp:label id="lblBrowserDirs" runat="server">Use Subdirs for non Admins?</asp:label></td>
        <td class="settingValueColumn"><asp:CheckBox ID="cbBrowserDirs" runat="server"></asp:CheckBox></td>
      </tr>
      <tr>
        <td class="settingNameColumn"><asp:label id="UploadFolderLabel" runat="server">Default Upload Folder</asp:label></td>
        <td class="settingValueColumn"><asp:DropDownList ID="UploadDir" runat="server" CssClass="DefaultDropDown"></asp:DropDownList></td>
      </tr>
      <tr>
        <td class="settingNameColumn"><asp:label id="OverrideFileOnUploadLabel" runat="server">Override File on Upload?</asp:label></td>
        <td class="settingValueColumn"><asp:Checkbox ID="OverrideFileOnUpload" runat="server"></asp:Checkbox></td>
      </tr>
      <tr>
	    <td class="settingNameColumn"><asp:label id="UploadFileLimitLabel" runat="server">Upload File Limits:</asp:label></td>
	    <td class="settingValueColumn">
	      <asp:GridView id="UploadFileLimits" runat="server" AutoGenerateColumns="False" Width="400px" GridLines="None" HeaderStyle-HorizontalAlign="Left" HeaderStyle-Font-Bold="false" HeaderStyle-Font-Italic="true">
            <Columns>
              <asp:TemplateField>
                <HeaderStyle HorizontalAlign="Left"></HeaderStyle>
                <HeaderTemplate>
                  <asp:Label id="lblRole" runat="server"></asp:Label>
                </HeaderTemplate>
                <ItemStyle Width="200"></ItemStyle>
                <ItemTemplate>
                  <asp:Label ID="lblRoleName" runat="server" Text="<%# Container.DataItem.ToString()%>"></asp:Label>
                </ItemTemplate>
              </asp:TemplateField>
              <asp:TemplateField>
                <HeaderStyle HorizontalAlign="Left"></HeaderStyle>                  
                <HeaderTemplate>
                  <asp:Label id="SizeLimitLabel" runat="server"></asp:Label>
                </HeaderTemplate>
                <ItemTemplate>
                  <asp:TextBox ID="SizeLimit" runat="server" CssClass="settingValueInputNumeric" Text="-1" />
                </ItemTemplate>
              </asp:TemplateField>
            </Columns>
          </asp:GridView>
        </td>
      </tr>
      <tr>
        <td class="settingNameColumn"><asp:label id="lblResizeWidth" runat="server">Default Image Resize Width:</asp:label></td>
        <td class="settingValueColumn"><asp:TextBox ID="txtResizeWidth" runat="server" CssClass="settingValueInputNumeric" />px</td>
      </tr>
      <tr>
        <td class="settingNameColumn"><asp:label id="lblResizeHeight" runat="server">Default Image Resize Height:</asp:label></td>
        <td class="settingValueColumn"><asp:TextBox ID="txtResizeHeight" runat="server" CssClass="settingValueInputNumeric" />px</td>
      </tr>
      <tr>
        <td class="settingNameColumn"><asp:label id="lblUseAnchorSelector" runat="server">Use Anchor Selector</asp:label></td>
        <td class="settingValueColumn"><asp:CheckBox ID="UseAnchorSelector" runat="server" Checked="True"></asp:CheckBox></td>
      </tr>
      <tr>
        <td class="settingNameColumn"><asp:label id="lblShowPageLinksTabFirst" runat="server">Show Page Links Tab First</asp:label></td>
        <td class="settingValueColumn"><asp:CheckBox ID="ShowPageLinksTabFirst" runat="server"></asp:CheckBox></td>
      </tr>
      <tr>
        <td class="settingNameColumn"><asp:label id="FileListViewModeLabel" runat="server">File List View Mode</asp:label></td>
        <td class="settingValueColumn"><asp:dropdownlist id="FileListViewMode" runat="server" CssClass="DefaultDropDown">
	        <asp:ListItem Text="DetailView" Value="DetailView" Enabled="true"></asp:ListItem>
	        <asp:ListItem Text="ListView" Value="ListView" ></asp:ListItem>
	        <asp:ListItem Text="IconsView" Value="IconsView"></asp:ListItem>
	      </asp:dropdownlist>
        </td>
      </tr>
      <tr>
        <td class="settingNameColumn"><asp:label id="FileListPageSizeLabel" runat="server">File List Page Size</asp:label></td>
        <td class="settingValueColumn"><asp:TextBox ID="FileListPageSize" runat="server" CssClass="settingValueInputNumeric"></asp:TextBox></td>
      </tr>
      <tr>
        <td class="settingNameColumn">
            <asp:label id="DefaultLinkModeLabel" runat="server">File List View Mode</asp:label>
        </td>
        <td class="settingValueColumn">
            <asp:dropdownlist id="DefaultLinkMode" runat="server" CssClass="DefaultDropDown">
                <asp:ListItem Text="RelativeURL" Value="RelativeURL" Enabled="true"></asp:ListItem>
                <asp:ListItem Text="Absolute URL" Value="AbsoluteURL" ></asp:ListItem>
                <asp:ListItem Text="RelativeSecuredURL" Value="RelativeSecuredURL"></asp:ListItem>
                <asp:ListItem Text="Absolute Secured" Value="AbsoluteSecuredURL"></asp:ListItem>
	      </asp:dropdownlist>
        </td>
      </tr>
    </table>
 
        </div>
        <!-- BEGIN Editor Config Tab -->  
        <div id="EditorConfigSetTab">
            <div class="ui-widget">
                <div class="ui-state-error ui-corner-all" style="padding: 0 .7em;">
                    <p><span class="ui-icon ui-icon-alert" style="float: left; margin-right: .3em;"></span>
                        <asp:Label runat="server" ID="EditorConfigWarning"></asp:Label>
                    </p>
                </div>

            </div>
            <asp:Panel runat="server" ID="EditorConfigHolder"></asp:Panel>
        </div>
        <!-- END Editor Config Tab -->  
            
        <!-- BEGIN Toolbars Tab -->  
        <div id="ToolbarsSetTab">
            <div class="settingNameContainer">
                <asp:label id="lblToolbarList" runat="server">Custom Toolbars List</asp:label>
            </div>
            <div class="settingValueContainer">
                <asp:DropDownList id="dDlCustomToolbars" runat="server"></asp:DropDownList>
            </div>
            <div class="settingNameContainer">
            </div>
            <div class="settingValueContainer">
                <asp:ImageButton id="iBEdit" runat="server" ImageUrl="~/images/edit.gif" CssClass="DefaultButton" />
                <asp:ImageButton id="iBDelete" runat="server" ImageUrl="~/images/delete.gif" CssClass="DefaultButton" />
                
            </div>
            <div class="settingNameContainer">
                <asp:label id="lblToolbName" runat="server">Add/Edit Toolbar Name</asp:label>
            </div>
            <div class="settingValueContainer">
                <asp:TextBox id="dnnTxtToolBName" runat="server" />
            </div>
            <div class="settingNameContainer">
            </div>
            <div class="settingValueContainer">
                <asp:ImageButton id="iBAdd" runat="server" ImageUrl="~/images/add.gif" CssClass="DefaultButton" />
                <asp:ImageButton id="iBCancel" runat="server" ImageUrl="~/images/cancel.gif" Visible="false" CssClass="DefaultButton" />
            </div>
            <table>
                <tr>
                   <td class="settingValueColumn">
                   <strong><asp:label id="lblToolbSet" runat="server">Available Toolbars:</asp:label></strong><br />
           <asp:Repeater ID="AvailableToolbarButtons" runat="server">
               <HeaderTemplate>
                   <ul class="availableButtons sortable">
               </HeaderTemplate>
               <ItemTemplate>
                   <li class='ui-state-default ui-corner-all<%# DataBinder.Eval(Container.DataItem, "Button").ToString().Equals("-") ? " separator" : string.Empty%>'>
                       <img alt='<%# DataBinder.Eval(Container.DataItem, "Button").ToString()%>' class="itemIcon" src='<%# this.ResolveUrl(string.Format("~/Providers/HtmlEditorProviders/CKEditor/icons/{0}", DataBinder.Eval(Container.DataItem, "Icon")))%>'/>&nbsp;
                       <span class="item"><%# DataBinder.Eval(Container.DataItem, "Button").ToString()%></span>
                   </li>
               </ItemTemplate>
               <FooterTemplate>
                   </ul>
               </FooterTemplate>
           </asp:Repeater>
        </td>
        <td style="vertical-align: top">
            <strong><asp:label id="ToolbarGroupsLabel" runat="server">Toolbar Groups:</asp:label></strong><br />
            <asp:HiddenField ID="ToolbarSet" runat="server" Value=""/>
            <asp:Repeater ID="ToolbarGroupsRepeater" runat="server">
                <HeaderTemplate>
                  <ul class="groups">
                </HeaderTemplate>
                <ItemTemplate>
                  <li class="groupItem<%# DataBinder.Eval(Container.DataItem, "GroupName").ToString().Equals("rowBreak") ? " rowBreakItem" : string.Empty%>">
                      <span class="ui-icon ui-icon-cancel" title="Delete this Toolbar Group"></span>
                      <span class="ui-icon ui-icon-arrowthick-2-n-s"></span>
                      <asp:HiddenField id="GroupListItem" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "GroupName").ToString()%>' />
                      <p class="rowBreakLabel"><%= DotNetNuke.Services.Localization.Localization.GetString("RowBreak.Text", this.ResXFile, this.LangCode) %></p>
                      <a class="groupName" title='<%= DotNetNuke.Services.Localization.Localization.GetString("EditGroupName.Text", this.ResXFile, this.LangCode) %>'><%# DataBinder.Eval(Container.DataItem, "GroupName").ToString()%></a>
                      <input type="text" class="groupEdit"><div class="ui-state-default ui-corner-all saveGroupName"><span class="ui-icon ui-icon-check" title="<%= DotNetNuke.Services.Localization.Localization.GetString("SaveGroupName.Text", this.ResXFile, this.LangCode) %>"></span></div>
                      <asp:Repeater ID="ToolbarButtonsRepeater" runat="server">
                          <HeaderTemplate>
                              <ul class="groupButtons">
                          </HeaderTemplate>
                          <ItemTemplate>
                              <li class='groupButton ui-state-default ui-corner-all<%# DataBinder.Eval(Container.DataItem, "Button").ToString().Equals("-") ? " separator" : string.Empty%><%# DataBinder.Eval(Container.DataItem, "Button").ToString().Equals("/") ? " rowBreak" : string.Empty%>'>
                                  <span class="ui-icon ui-icon-cancel" title='<%= DotNetNuke.Services.Localization.Localization.GetString("DeleteToolbarButton.Text", this.ResXFile, this.LangCode) %>'></span>
                                  <img alt='<%# DataBinder.Eval(Container.DataItem, "Button").ToString()%>' class="itemIcon" src='<%# this.ResolveUrl(string.Format("~/Providers/HtmlEditorProviders/CKEditor/icons/{0}", DataBinder.Eval(Container.DataItem, "Button").ToString().Equals("/") ? "PageBreak.png" : DataBinder.Eval(Container.DataItem, "Icon")))  %>'/>&nbsp;
                                  <span class="item"><%# DataBinder.Eval(Container.DataItem, "Button").ToString()%></span>
                              </li>
                          </ItemTemplate>
                          <FooterTemplate>
                              </ul>
                          </FooterTemplate>
                      </asp:Repeater>
                  </li>
                </ItemTemplate>
                <FooterTemplate>
                  </ul>
                </FooterTemplate>
              </asp:Repeater>
              <div class="ui-state-default ui-corner-all createGroupButton" id="createGroup" title='<%= DotNetNuke.Services.Localization.Localization.GetString("CreateGroupTitle.Text", this.ResXFile, this.LangCode) %>'>
                  <span class="ui-icon ui-icon-document" style="display:inline-block"></span>
                  <asp:Label ID="CreateGroupLabel" runat="server">Create New Group</asp:Label>
              </div>
              <div class="ui-state-default ui-corner-all addRowBreakButton" id="addRowBreak" title='<%= DotNetNuke.Services.Localization.Localization.GetString("AddRowBreakTitle.Text", this.ResXFile, this.LangCode) %>'>
                  <span class="ui-icon ui-icon-grip-dotted-horizontal" style="display:inline-block"></span>
                  <asp:Label ID="AddRowBreakLabel" runat="server">Add Row Break</asp:Label>
              </div>
        </td>
      </tr>
      </table>
         <div class="settingNameContainer">
           <asp:label id="lblToolbarPriority" runat="server">Toolbar Set Priority</asp:label>
         </div>
         <div class="settingValueContainer">
          <asp:DropDownList id="dDlToolbarPrio" runat="server">
                      <asp:ListItem Text="01" Value="01"></asp:ListItem>
            <asp:ListItem Text="02" Value="02"></asp:ListItem>
            <asp:ListItem Text="03" Value="03"></asp:ListItem>
            <asp:ListItem Text="04" Value="04"></asp:ListItem>
            <asp:ListItem Text="05" Value="05"></asp:ListItem>
            <asp:ListItem Text="06" Value="06"></asp:ListItem>
            <asp:ListItem Text="07" Value="07"></asp:ListItem>
            <asp:ListItem Text="08" Value="08"></asp:ListItem>
            <asp:ListItem Text="09" Value="09"></asp:ListItem>
            <asp:ListItem Text="10" Value="10"></asp:ListItem>
            <asp:ListItem Text="11" Value="11"></asp:ListItem>
            <asp:ListItem Text="12" Value="12"></asp:ListItem>
            <asp:ListItem Text="13" Value="13"></asp:ListItem>
            <asp:ListItem Text="14" Value="14"></asp:ListItem>
            <asp:ListItem Text="15" Value="15"></asp:ListItem>
            <asp:ListItem Text="16" Value="16"></asp:ListItem>
            <asp:ListItem Text="17" Value="17"></asp:ListItem>
            <asp:ListItem Text="18" Value="18"></asp:ListItem>
            <asp:ListItem Text="19" Value="19"></asp:ListItem>
            <asp:ListItem Text="20" Value="20"></asp:ListItem>
          </asp:DropDownList>
             </div>
          </div>
          <!-- / Toolbars Set Tab -->
        </div>
      </td>
    </tr>
  </table>
</div>

<div>
  </ContentTemplate>
        </asp:UpdatePanel>
  <asp:Button id="btnOk" CssClass="DefaultButton ui-state-focus" runat="server" Text="OK" />&nbsp;<asp:Button id="btnCancel" CssClass="DefaultButton" runat="server" Text="Close" />
</div>

<!-- Loading screen -->
<asp:Panel id="panelLoading" CssClass="panelLoading" runat="server">
    <div class="ModalDialog_overlayBG" id="LoadingScreen" ></div>
    <div class="MessageBox">
        <div class="ModalDialog">
            <div class="popup">
                <div class="DialogContent LoadingContent">
                    <div class="modalHeader">
                        <h3><asp:Label id="Wait" runat="server" Text="Please Wait" /></h3>
                    </div>
                    <div class="LoadingMessage"><asp:Label id="WaitMessage" runat="server" Text="Loading Page"></asp:Label></div>
                </div>
            </div>
        </div>
    </div>
</asp:Panel>
<!-- / Loading screen -->