<%@ Control Language="c#" AutoEventWireup="True" Codebehind="CKEditorOptions.ascx.cs" Inherits="DNNConnect.CKEditorProvider.CKEditorOptions" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Register TagPrefix="dnn" TagName="URL" Src="UrlControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls"%>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/LabelControl.ascx" %>

<div id="SettingsBox">
  <h2>CKEditor Provider <asp:Label id="lblSettings" runat="server">Settings</asp:Label></h2>    

    <asp:UpdatePanel ID="upOptions" UpdateMode="Conditional" runat="server">
        <ContentTemplate>
        <asp:HiddenField id="LastTabId" runat="server" Value="1"/>           
		<asp:Panel ID="pnlEditor" runat="server" CssClass="dnnForm">

            <div class="SettingsHeader">
                <div class="dnnFormItem">
                    <asp:Label id="lblSetFor" runat="server" CssClass="dnnLabel">Modify Settings for:</asp:Label>
                    <asp:RadioButtonList id="rBlSetMode" runat="server" RepeatDirection="Horizontal" AutoPostBack="true">
                        <asp:ListItem Text="Portal" Value="portal" Selected="True"></asp:ListItem>
                        <asp:ListItem Text="Page" Value="page"></asp:ListItem>
                        <asp:ListItem Text="Module Instance" Value="minstance"></asp:ListItem>
                    </asp:RadioButtonList>
                </div>
            </div>                
                               
			<ul class="dnnAdminTabNav dnnClear">
				<li><a href="#dnnMainSettings"><asp:label id="lblMainSet" runat="server">Main Settings</asp:label></a></li>
				<li><a href="#dnnFileBrowserSettings"><asp:label id="lblBrowsSec" runat="server">File Browser Settings</asp:label></a></li>
				<li><a href="#dnnCustomToolbars"><asp:label id="lblCustomToolbars" runat="server">Custom Toolbars</asp:label></a></li>
				<li><a href="#dnnEditorConfig"><asp:label id="lblEditorConfig" runat="server">Editor Config</asp:label></a></li>
				<li runat="server" id="InfoTabLi"><a href="#dnnAbout"><asp:Label id="lblHeader" runat="server">About</asp:Label></a></li>
			</ul>
            <div id="dnnMainSettings">     
                          
			  <h2 id="Panel-Common" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("Common.SectionName")%>Common</a></h2>
			  <fieldset>                
				<div class="dnnFormItem">
					<asp:label id="lblBlanktext" runat="server" CssClass="dnnLabel">Blank Text:</asp:label>
				    <asp:TextBox runat="server" id="txtBlanktext" />
				</div>
				<div class="dnnFormItem">
                    <asp:label id="lblSkin" runat="server" CssClass="dnnLabel">Skin:</asp:label>
                    <asp:dropdownlist id="ddlSkin" runat="server" cssclass="dnnDropDownList"></asp:dropdownlist>
                </div>
				<div class="dnnFormItem">
				    <asp:label id="lblWidth" runat="server" CssClass="dnnLabel">Editor Width:</asp:label>
                    <asp:TextBox runat="server" id="txtWidth" CssClass="settingValueInputNumeric" />
                </div>
				<div class="dnnFormItem">
				    <asp:label id="lblHeight" runat="server" CssClass="dnnLabel">Editor Height:</asp:label>      
                    <asp:TextBox runat="server" id="txtHeight" CssClass="settingValueInputNumeric" />                
                </div>
              </fieldset>
                
			    <h2 id="Panel-Toolbars" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("Toolbars.SeectionName")%><asp:label id="lblToolbars" runat="server">Toolbars</asp:label></a></h2>
			    <fieldset>
	                <asp:GridView id="gvToolbars" runat="server" AutoGenerateColumns="False" GridLines="None">
                    <Columns>
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <asp:Label id="lblRole" runat="server" visible="false"></asp:Label>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:Label ID="lblRoleName" runat="server" Text="<%# Container.DataItem.ToString()%>" cssclass="dnnLabel"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <asp:Label id="lblSelToolb" runat="server" visible="false"></asp:Label>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:dropdownlist id="ddlToolbars" runat="server" cssclass="dnnDropDownList"></asp:dropdownlist>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    </asp:GridView>               
                </fieldset>

			    <h2 id="Panel-Configuration" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("Configuration.SeectionName")%>Configuration</a></h2>
			    <fieldset>
				    <div class="dnnFormItem">
				        <asp:label id="lblCustomConfig" runat="server" CssClass="dnnLabel">Custom Config File</asp:label> 
                            <dnn:url id="ctlConfigUrl" runat="server" showtabs="False" Required="False" filefilter="js" showupload="False" showfiles="True" showUrls="True"
					                urltype="F" showlog="False" shownewwindow="False" showtrack="False"></dnn:url>                 
                    </div>
                    <div class="dnnFormItem">
                        <asp:label id="CodeMirrorLabel" runat="server" CssClass="dnnLabel">CodeMirror Theme (for Source Syntax Highlighting):</asp:label>
                        <asp:dropdownlist id="CodeMirrorTheme" runat="server"></asp:dropdownlist>
                    </div>
				    <div class="dnnFormItem">
				        <asp:label id="lblInjectSyntaxJs" runat="server" CssClass="dnnLabel">Inject Syntax Highlighter Js Code?</asp:label>   
                        <asp:CheckBox ID="InjectSyntaxJs" runat="server" Checked="true"></asp:CheckBox>                  
                    </div>
				    <div class="dnnFormItem">
				        <asp:label id="lblCssurl" runat="server" CssClass="dnnLabel">Editor area CSS</asp:label>   
                            <dnn:url id="ctlCssurl" runat="server" showtabs="False" Required="False" filefilter="css" showupload="False" showfiles="True" showUrls="True"
					            urltype="F" showlog="False" shownewwindow="False" showtrack="False"></dnn:url>   
                    </div>
				    <div class="dnnFormItem">
                        <asp:label id="lblTemplFiles" runat="server" CssClass="dnnLabel">Editor Templates File</asp:label>	
                            <dnn:url id="ctlTemplUrl" runat="server" showtabs="False" Required="False" filefilter="xml,js" showupload="False" showfiles="True" showUrls="True"
					            urltype="F" showlog="False" shownewwindow="False" showtrack="False"></dnn:url>    
                    </div>
                    <div class="dnnFormItem">
                        <asp:label id="CustomJsFileLabel" runat="server" CssClass="dnnLabel">Custom JS File</asp:label>
                            <dnn:url id="ctlCustomJsFile" runat="server" showtabs="False" Required="False" filefilter="js" showupload="False" showfiles="True" showUrls="True"
					            urltype="F" showlog="False" shownewwindow="False" showtrack="False"></dnn:url>
                    </div>
                </fieldset>

            </div>
			<div id="dnnFileBrowserSettings">
			    <h2 id="Panel-FileBrowser" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("FileBrowser.SeectionName")%>File Browser Settings</a></h2>
			    <fieldset>
				    <div class="dnnFormItem">
					    <asp:label id="lblBrowser" runat="server" CssClass="dnnLabel">File Browser:</asp:label>
	                      <asp:dropdownlist id="ddlBrowser" runat="server" CssClass="DefaultDropDown">
	                        <asp:ListItem Text="None" Value="none" Enabled="true"></asp:ListItem>
	                        <asp:ListItem Text="Standard" Value="standard" ></asp:ListItem>
	                        <asp:ListItem Text="CKFinder" Value="ckfinder"></asp:ListItem>
	                      </asp:dropdownlist>
				    </div>
				    <div class="dnnFormItem">
                        <asp:label id="lblBrowAllow" runat="server" CssClass="dnnLabel">File Browser Security</asp:label>
                        <asp:CheckBoxList ID="chblBrowsGr" runat="server"></asp:CheckBoxList>
				    </div>
				    <div class="dnnFormItem">
                        <asp:label id="BrowserRootFolder" runat="server" CssClass="dnnLabel">Browser Root Folder</asp:label>
                        <asp:DropDownList ID="BrowserRootDir" runat="server" CssClass="DefaultDropDown"></asp:DropDownList>
				    </div>
				    <div class="dnnFormItem">
                        <asp:label id="lblBrowserDirs" runat="server" CssClass="dnnLabel">Use Subdirs for non Admins?</asp:label>
                        <asp:CheckBox ID="cbBrowserDirs" runat="server"></asp:CheckBox>
				    </div>
				    <div class="dnnFormItem">
                        <asp:label id="UploadFolderLabel" runat="server" CssClass="dnnLabel">Default Upload Folder</asp:label>
                        <asp:DropDownList ID="UploadDir" runat="server" CssClass="DefaultDropDown"></asp:DropDownList>
				    </div>
				    <div class="dnnFormItem">
                        <asp:label id="OverrideFileOnUploadLabel" runat="server" CssClass="dnnLabel">Override File on Upload?</asp:label>
                        <asp:Checkbox ID="OverrideFileOnUpload" runat="server"></asp:Checkbox>
				    </div>
				</fieldset>
			    <h2 id="Panel-UploadFileLimits" class="dnnFormSectionHead">
                    <a href="" class="dnnSectionExpanded"><%=LocalizeString("UploadFileLimits.SeectionName")%>
                        <asp:label id="UploadFileLimitLabel" runat="server">Upload File Limits</asp:label>
                    </a>
			    </h2>
			    <fieldset>
	                <asp:GridView id="UploadFileLimits" runat="server" AutoGenerateColumns="False" GridLines="None">
                        <Columns>
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    <asp:Label id="lblRole" runat="server" visible="false"></asp:Label>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblRoleName" runat="server" Text="<%# Bind('RoleName') %>" cssclass="dnnLabel"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    <asp:Label id="SizeLimitLabel" runat="server" visible="false"></asp:Label>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:TextBox ID="SizeLimit" runat="server" CssClass="settingValueInputNumeric" Text="<%# Bind('UploadFileLimit') %>" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </fieldset>
			    <h2 id="Panel-FileBrowserDefaults" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("FileBrowserDefaults.SeectionName")%>Default Settings</a></h2>
			    <fieldset>
                    <div class="dnnFormItem">
                        <asp:label id="lblResizeWidth" runat="server" CssClass="dnnLabel">Default Image Resize Width:</asp:label>
                        <asp:TextBox ID="txtResizeWidth" runat="server" CssClass="settingValueInputNumeric" />px
                    </div>
                    <div class="dnnFormItem">
                        <asp:label id="lblResizeHeight" runat="server" CssClass="dnnLabel">Default Image Resize Height:</asp:label>
                        <asp:TextBox ID="txtResizeHeight" runat="server" CssClass="settingValueInputNumeric" />px
                    </div>
                    <div class="dnnFormItem">
                        <asp:label id="lblUseAnchorSelector" runat="server" CssClass="dnnLabel">Use Anchor Selector</asp:label>
                        <asp:CheckBox ID="UseAnchorSelector" runat="server" Checked="True"></asp:CheckBox>
                    </div>
                    <div class="dnnFormItem">
                        <asp:label id="lblShowPageLinksTabFirst" runat="server" CssClass="dnnLabel">Show Page Links Tab First</asp:label>
                        <asp:CheckBox ID="ShowPageLinksTabFirst" runat="server"></asp:CheckBox>
                    </div>
                    <div class="dnnFormItem">
                        <asp:label id="FileListViewModeLabel" runat="server" CssClass="dnnLabel">File List View Mode</asp:label>
                        <asp:dropdownlist id="FileListViewMode" runat="server">
	                        <asp:ListItem Text="DetailView" Value="DetailView" Enabled="true"></asp:ListItem>
	                        <asp:ListItem Text="ListView" Value="ListView" ></asp:ListItem>
	                        <asp:ListItem Text="IconsView" Value="IconsView"></asp:ListItem>
	                      </asp:dropdownlist>
                    </div>
                    <div class="dnnFormItem">
                        <asp:label id="FileListPageSizeLabel" runat="server" CssClass="dnnLabel">File List Page Size</asp:label>
                        <asp:TextBox ID="FileListPageSize" runat="server" CssClass="settingValueInputNumeric"></asp:TextBox>
                    </div>
                    <div class="dnnFormItem">
                        <asp:label id="DefaultLinkModeLabel" runat="server" CssClass="dnnLabel">File List View Mode</asp:label>
                        <asp:dropdownlist id="DefaultLinkMode" runat="server">
                            <asp:ListItem Text="RelativeURL" Value="RelativeURL" Enabled="true"></asp:ListItem>
                            <asp:ListItem Text="Absolute URL" Value="AbsoluteURL" ></asp:ListItem>
                            <asp:ListItem Text="RelativeSecuredURL" Value="RelativeSecuredURL"></asp:ListItem>
                            <asp:ListItem Text="Absolute Secured" Value="AbsoluteSecuredURL"></asp:ListItem>
	                    </asp:dropdownlist>
                    </div>        
                </fieldset>                    
			</div>
			<div id="dnnCustomToolbars">
				<div class="dnnFormItem">
				    <asp:label id="lblToolbarList" runat="server" CssClass="dnnLabel">Custom Toolbars List</asp:label>
				    <asp:DropDownList id="dDlCustomToolbars" runat="server"></asp:DropDownList>
                    <asp:ImageButton id="iBEdit" runat="server" ImageUrl="~/images/edit.gif" CssClass="DefaultButton" Text="Edit" />
                    <asp:ImageButton id="iBDelete" runat="server" ImageUrl="~/images/delete.gif" CssClass="DefaultButton" Text="Delete" />                   
				</div>
                <div class="dnnFormItem">
                    <asp:label id="lblToolbName" runat="server" CssClass="dnnLabel">Add/Edit Toolbar Name</asp:label>
                    <asp:TextBox id="dnnTxtToolBName" runat="server" />
                    <asp:ImageButton id="iBAdd" runat="server" ImageUrl="~/images/add.gif" CssClass="DefaultButton" Text="Add" />
                    <asp:ImageButton id="iBCancel" runat="server" ImageUrl="~/images/cancel.gif" Visible="false" CssClass="DefaultButton" Text="Cancel" />
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
                                    <img alt='<%# DataBinder.Eval(Container.DataItem, "Button").ToString()%>' class="itemIcon" src='<%# this.ResolveUrl(string.Format("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.5.3/icons/{0}", DataBinder.Eval(Container.DataItem, "Icon")))%>'/>&nbsp;
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
                                              <img alt='<%# DataBinder.Eval(Container.DataItem, "Button").ToString()%>' class="itemIcon" src='<%# this.ResolveUrl(string.Format("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.5.3/icons/{0}", DataBinder.Eval(Container.DataItem, "Button").ToString().Equals("/") ? "PageBreak.png" : DataBinder.Eval(Container.DataItem, "Icon")))  %>'/>&nbsp;
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
            
                <div class="dnnFormItem">
                    <asp:label id="lblToolbarPriority" runat="server" CssClass="dnnLabel">Toolbar Set Priority</asp:label>
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
			<div id="dnnEditorConfig">
                <asp:Label runat="server" ID="EditorConfigWarning" CssClass="dnnFormMessage dnnFormWarning"></asp:Label>
                <asp:Panel runat="server" ID="EditorConfigHolder"></asp:Panel>
			</div>
			<div id="dnnAbout">
                <asp:PlaceHolder runat="server" ID="InfoTabHolder">
                <ul>
                    <li><asp:Label id="ProviderVersion" runat="server">Editor Provider Version:</asp:Label></li>
                    <li><asp:Label id="lblPortal" runat="server">Portal:</asp:Label></li>
                    <li><asp:Label id="lblPage" runat="server">Page:</asp:Label></li>
                    <li><asp:Label id="lblModType" runat="server">Module type:</asp:Label></li>
                    <li><asp:Label id="lblModName" runat="server">Module Name:</asp:Label></li>
                    <li><asp:Label id="lblModInst" runat="server">Module Instance:</asp:Label></li>
                    <li><asp:Label id="lblUName" runat="server">User Name:</asp:Label></li>
                </ul>
                </asp:PlaceHolder>
			</div>

        </asp:Panel>

        <ul class="dnnActions dnnClear">
            <li><asp:Button id="btnOk" runat="server" Text="OK" CssClass="dnnPrimaryAction" /></li>
	        <li>
	            <% if (!IsHostMode) { %>
	                <a href="#" onclick="window.close();" class="dnnSecondaryAction"><%= LocalizeString("btnCancel.Text") %></a>
                <% } %>
	        </li>
        </ul>
            
        <ul class="dnnActions dnnClear">
            <li><asp:LinkButton id="CopyToAllChild" runat="server" Text="Copy Settings to Child Pages" CssClass="dnnSecondaryAction copyButton"></asp:LinkButton></li>
            <li><asp:LinkButton id="lnkRemoveAll" runat="server" Text="Delete All Settings" CssClass="dnnSecondaryAction removeButton"></asp:LinkButton></li>
            <li><asp:LinkButton id="lnkRemoveChild" runat="server" Text="Delete Child Settings" CssClass="removeButton dnnSecondaryAction"></asp:LinkButton></li>
            <li><asp:LinkButton id="lnkRemove" runat="server" Text="Delete Settings" CssClass="dnnSecondaryAction removeButton"></asp:LinkButton></li>
            <li><a onclick="showDialog('ImportDialog');" id="ckeditoroptions_lnkImport" class="dnnSecondaryAction importButton" href="#"><asp:Label id="lblImport" runat="server" Text="Import"></asp:Label></a></li>
            <li><a onclick="showDialog('ExportDialog');" id="ckeditoroptions_Export" href="#" class="dnnSecondaryAction exportButton"><asp:Label id="lblExport" runat="server" Text="Export"></asp:Label></a></li>
        </ul>

        </ContentTemplate>
    </asp:UpdatePanel>
</div>

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

<script language="javascript" type="text/javascript">
    /*globals jQuery, window, Sys */
    (function ($, Sys) {
        function setupDnnProviderConfig() {
            $('#<%=pnlEditor.ClientID%>').dnnTabs().dnnPanels();
	    }

	    $(document).ready(function () {
	        setupDnnProviderConfig();
	        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
	            setupDnnProviderConfig();
	        });
	    });

	}(jQuery, window.Sys));
</script>