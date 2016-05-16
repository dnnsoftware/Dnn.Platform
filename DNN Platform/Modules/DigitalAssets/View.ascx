<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="View.ascx.cs" Inherits="DotNetNuke.Modules.DigitalAssets.View" %>

<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="DotNetNuke.Services.FileSystem" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Import Namespace="DotNetNuke.UI.Utilities" %>
<%@ Import Namespace="DotNetNuke.Entities.Icons" %>

<%@ Register TagPrefix="dnnext" Namespace="DotNetNuke.ExtensionPoints" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnnweb" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnnweb" Assembly="DotNetNuke.Web.Deprecated" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnnweb" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<asp:Panel ID="ScopeWrapper" runat="server">
    
    <div class="dnnModuleDigitalAssetsBackground" oncontextmenu="return false;">
        
        <div class="dnnModuleDigitalAssetsLoading dnnModuleDigitalAssetsMainLoading"></div>
        <dnnext:EditPageTabExtensionControl runat="server" Module="DigitalAssets" Group="LeftPaneTabs" TabControlId="LeftPaneTabsControl" PanelControlId="LeftPaneContents"></dnnext:EditPageTabExtensionControl>
        <div id="dnnModuleDigitalAssetsLeftPane">
            <ul class="dnnAdminTabNav dnnModuleDigitalAssetsTabNav buttonGroup" runat="server" id="LeftPaneTabsControl">
                <li>
                    <asp:HyperLink href="#dnnModuleDigitalAssetsLeftPaneFilesTabContent" runat="server" resourcekey="LeftPaneFilesTab.Text"/>            
                </li>
            </ul>
            <asp:Panel runat="server" ID="LeftPaneContents" CssClass="dnnModuleDigitalAssetsLeftPaneContents">
                <div class="dnnModuleDigitalAssetsFilesView" id="dnnModuleDigitalAssetsLeftPaneFilesTabContent">
                    <div id="dnnModuleDigitalAssetsLeftPaneFilesTabContentScroll">
                        <dnnweb:DnnTreeView ID="FolderTreeView" runat="server" CssClass="dnnModuledigitalAssetsTreeView" 
                            OnClientNodeExpanding="dnnModule.digitalAssets.treeViewOnNodeExpanding" 
                            OnClientNodeCollapsing="dnnModule.digitalAssets.treeViewOnNodeCollapsing"
                            OnClientNodeClicking="dnnModule.digitalAssets.treeViewOnNodeClicking" 
                            OnClientNodeAnimationEnd="dnnModule.digitalAssets.treeViewRefreshScrollbars"
                            OnClientContextMenuItemClicking="dnnModule.digitalAssets.treeViewOnContextMenuItemClicking"
                            OnClientContextMenuShowing="dnnModule.digitalAssets.treeViewOnContextMenuShowing"
                            OnClientNodeEditing="dnnModule.digitalAssets.treeViewOnNodeEditing"
                            OnClientContextMenuShown="dnnModule.digitalAssets.treeViewOnContextMenuShown"
                            OnClientLoad="dnnModule.digitalAssets.treeViewOnLoad">  
                            <ContextMenus>
                                <dnnweb:DnnTreeViewContextMenu ID="MainContextMenu" runat="server" CssClass="dnnModuleDigitalAssetsContextMenu" 
                                    OnClientLoad="dnnModule.digitalAssets.treeViewContextMenuOnLoad" 
                                    OnClientHiding="dnnModule.digitalAssets.treeViewContextMenuOnHiding" >
                                    <CollapseAnimation Type="none"></CollapseAnimation>
                                </dnnweb:DnnTreeViewContextMenu>
                            </ContextMenus>
                        </dnnweb:DnnTreeView>
                    </div>
                    <ul id="dnnModuleDigitalAssetsLeftPaneActions"></ul>
                </div>
            </asp:Panel>
        </div>

        <div id="dnnModuleDigitalAssetsContentPane">
                        
            <asp:Panel runat="server" ID="SearchBoxPanel" />                
            
            <div id="dnnModuleDigitalAssetsBreadcrumb">
                <ul></ul>
            </div>
            <div id="dnnModuleDigitalAssetsMainToolbarTitle">
                <span class="title-views"><%=Localization.GetString("ToolbarTitle.Views.Text", LocalResourceFile)%></span>
                <span class="title-actions"><%=Localization.GetString("ToolbarTitle.Actions.Text", LocalResourceFile)%></span>
                <span class="title-currentFolder"></span>
            </div>
            <div id="dnnModuleDigitalAssetsMainToolbar">                
                <dnnext:ToolBarButtonExtensionControl Module="DigitalAssets" runat="server" ID="MainToolBar" Group="Main" IsHost="<%# IsHostPortal %>" />                
            </div>
            <div id="dnnModuleDigitalAssetsSelectionToolbar">
                <span id="dnnModuleDigitalAssetsSelectionText"></span>
                <dnnext:ToolBarButtonExtensionControl Module="DigitalAssets" runat="server" ID="SelectionToolBar" Group="Selection" IsHost="<%# IsHostPortal %>" />
            </div>
            
            <div id="dnnModuleDigitalAssetsListContainer" class="emptySpace"> 
                
                <div id="dnnModuleDigitalAssetsListViewContainer" style="display: none" class="emptySpace">                    
                    <div id="dnnModuleDigitalAssetsListViewToolbar">
                        <input type="checkbox" />
                        <span class="dnnModuleDigitalAssetsListViewToolbarTitle"><%=Localization.GetString("SelectAll", LocalResourceFile)%></span>
                    </div>      
                    <div id="dnnModuleDigitalAssetsListViewNoItems" style="display: none;" class="emptySpace">
                        <span class="dnnModuleDigitalAssetsNoItems emptySpace"></span>
                    </div>
                    <dnnweb:DnnListView runat="server" Id="FolderListView">
                        <ClientSettings>
                            <DataBinding ItemPlaceHolderID="dnnModuleDigitalAssetsListView">
                                <LayoutTemplate>
                                    <div id="dnnModuleDigitalAssetsListView" class="emptySpace"></div>
                                </LayoutTemplate>
                                <ItemTemplate>
                                    <div id="dnnModuleDigitalAssetsListViewItem_#= index #" class="dnnModuleDigitalAssetsListViewItem" data-index="#= index #"
                                        oncontextmenu="dnnModule.digitalAssets.listviewOnContextMenu(this, event);" 
                                        onclick="dnnModule.digitalAssets.listviewOnClick(this, event);">
                                        <input type="checkbox" class="dnnModuleDigitalAssetsListViewItemCheckBox" />
                                        <div class="dnnModuleDigitalAssetsThumbnail">
                                            <img data-src="#= format(ThumbnailUrl) #" class="#= format(ThumbnailClass) #" alt="#= format(ItemName) #"/>
                                        </div>
                                        <span class="dnnModuleDigitalAssetsListViewItemLinkName" title="#= format(ItemName) #" >#= dnnModule.digitalAssets.highlightItemName( dnnModule.digitalAssets.getReducedItemName(ItemName, IsFolder) ) #</span>
                                    </div>
                                </ItemTemplate>
                            </DataBinding>
                            <ClientEvents OnListViewCreated="dnnModule.digitalAssets.listViewOnCreated" />
                        </ClientSettings>
                    </dnnweb:DnnListView>
                </div>   

                <dnnweb:DnnGrid runat="server" ID="Grid" AutoGenerateColumns="false" AllowRowSelect="True" AllowMultiRowSelection="True"
                    AllowPaging="True" AllowSorting="True" CssClass="dnnModuleDigitalAssetsGrid emptySpace" OnItemCreated="GridOnItemCreated">
                    <ClientSettings EnablePostBackOnRowClick="false" >
                        <Selecting AllowRowSelect="True" UseClientSelectColumnOnly="False" EnableDragToSelectRows="False" />
                        <ClientEvents 
                            OnGridCreated="dnnModule.digitalAssets.gridOnGridCreated" 
                            OnCommand="dnnModule.digitalAssets.gridOnCommand" 
                            OnRowContextMenu="dnnModule.digitalAssets.gridOnRowContextMenu"
                            OnRowSelected="dnnModule.digitalAssets.gridOnRowSelected" 
                            OnRowDeselected="dnnModule.digitalAssets.gridOnRowDeselected"
                            OnRowDataBound="dnnModule.digitalAssets.gridOnRowDataBound"
                            OnDataBound="dnnModule.digitalAssets.gridOnDataBound"
                            OnColumnHidden="dnnModule.digitalAssets.gridOnColumnHidden" /> 
                    </ClientSettings>
                    <MasterTableView TableLayout="Fixed" AllowCustomSorting="True" AllowSorting="true" EditMode="InPlace" EnableColumnsViewState="false">
                        <Columns>
                            <dnnweb:DnnGridClientSelectColumn HeaderStyle-Width="44px" UniqueName="Select" />                        
                            <dnnweb:DnnGridBoundColumn UniqueName="ItemName" SortExpression="ItemName" DataField="ItemName" HeaderText="Name"  HeaderStyle-Width="100%" />
                            <dnnweb:DnnGridBoundColumn UniqueName="LastModifiedOnDate" DataField="LastModifiedOnDate" HeaderText="Modified" HeaderStyle-Width="170px" ReadOnly="True" />
                            <dnnweb:DnnGridBoundColumn UniqueName="Size" DataField="Size" HeaderText="Size" Visible="True" ReadOnly="True" HeaderStyle-Width="80px" />
                            <dnnweb:DnnGridBoundColumn UniqueName="ItemID" DataField="ItemID" HeaderText="ItemID" Visible="False" ReadOnly="True"/>
                            <dnnweb:DnnGridBoundColumn UniqueName="IsFolder" DataField="IsFolder" HeaderText="IsFolder" Visible="False" ReadOnly="True"/>
                            <dnnweb:DnnGridBoundColumn UniqueName="ParentFolder" DataField="ParentFolder" HeaderText="ParentFolder" Visible="True" ReadOnly="True"/>
                        </Columns>
                        <NoRecordsTemplate>
                            <div id="dnnModuleDigitalAssetsGridViewNoItems" class="emptySpace">
                                <span class="dnnModuleDigitalAssetsNoItems emptySpace"></span>
                            </div>
                        </NoRecordsTemplate>
                        <PagerStyle AlwaysVisible="true" PageButtonCount="6" CssClass="dnnModuleDigitalAssetsPagerStyle" Mode="NextPrevAndNumeric"/>
                    </MasterTableView>
                </dnnweb:DnnGrid>            
            
            </div>

            <dnnweb:DnnContextMenu ID="GridMenu" runat="server" CssClass="dnnModuleDigitalAssetsContextMenu" 
                OnClientItemClicked="dnnModule.digitalAssets.contextMenuOnItemClicked" 
                OnClientShown="dnnModule.digitalAssets.contextMenuOnShown"
                OnClientLoad="dnnModule.digitalAssets.contextMenuOnLoad">
            </dnnweb:DnnContextMenu>         
            
            <dnnweb:DnnContextMenu ID="EmptySpaceMenu" runat="server" CssClass="dnnModuleDigitalAssetsContextMenu" 
                OnClientItemClicked="dnnModule.digitalAssets.emptySpaceMenuOnItemClicked" 
                OnClientLoad="dnnModule.digitalAssets.emptySpaceMenuOnLoad">
            </dnnweb:DnnContextMenu>   
                
        </div>  
    
    </div>

    <div id="dnnModuleDigitalAssetsCreateFolderModal" style="display: none;">
        <div class="dnnModuleDigitalAssetsLoading dnnModuleDigitalAssetsCreateFolderLoading" style="display: none; width: 100%;"></div>
        <div class="dnnClear">
            <fieldset>
                <div class="dnnFormItem">
                    <dnnweb:Label ID="ParentFolderLabel" runat="server" ResourceKey="ParentFolder" Suffix=":" HelpKey="ParentFolder.Help" ControlName="FolderNameTextBox" />
                    <span id="dnnModuleDigitalAssetsCreateFolderModalParent" class="dnnModuleDigitalAssetsCreateFolderModalNoEditableField"></span>                
                </div>
                <div class="dnnFormItem">
                    <dnnweb:Label ID="FolderNameLabel" runat="server" ResourceKey="FolderName" Suffix=":" HelpKey="FolderName.Help" ControlName="FolderNameTextBox" CssClass="dnnFormRequired" />
                    <asp:TextBox ID="FolderNameTextBox" runat="server" />
                    <asp:RequiredFieldValidator ID="FolderNameRequiredValidator" ValidationGroup="CreateFolder" CssClass="dnnFormMessage dnnFormError dnnModuleDigitalAssetsFolderNameValidator" EnableViewState="false" runat="server" resourcekey="FolderNameRequired.ErrorMessage" Display="Dynamic" ControlToValidate="FolderNameTextBox" />
                    <asp:RegularExpressionValidator Width="222" ID="FolderNameRegExValidator" ValidationGroup="CreateFolder" CssClass="dnnFormMessage dnnFormError dnnModuleDigitalAssetsFolderNameIvalidCharsValidator" EnableViewState="false" runat="server" Display="Dynamic" ControlToValidate="FolderNameTextBox" />
                </div>
                <div class="dnnFormItem">
                    <dnnweb:Label ID="FolderTypeLabel" runat="server" ResourceKey="FolderType" Suffix=":" HelpKey="FolderType.Help" ControlName="FolderTypeComboBox" />
                    <dnnweb:DnnComboBox id="FolderTypeComboBox" DataTextField="Name" DataValueField="Id" runat="server" OnClientSelectedIndexChanged="dnnModule.digitalAssets.folderTypeComboBoxOnSelectedIndexChanged"></dnnweb:DnnComboBox>
                    <span id="dnnModuleDigitalAssetsFolderTypeNoEditableLabel" class="dnnModuleDigitalAssetsCreateFolderModalNoEditableField"></span>
                </div>
                <div class="dnnFormItem" id="dnnModuleDigitalAssetsCreateFolderMappedPathFieldRow">                    
                    <dnnweb:Label ID="MappedPathLabel" runat="server" ResourceKey="MappedPath" Suffix=":" HelpKey="MappedPath.Help" ControlName="MappedPathTextBox" />
                    <asp:TextBox ID="MappedPathTextBox" runat="server" />                    
                    <asp:RegularExpressionValidator Width="222" ID="MappedPathRegExValidator" ValidationGroup="CreateFolder" 
                        CssClass="dnnFormMessage dnnFormError" EnableViewState="false" runat="server" Display="Dynamic" 
                        ControlToValidate="MappedPathTextBox" ValidationExpression="^(?!\s*[\\/]).*$" resourcekey="MappedPathRegExValidator.ErrorMessage"/>
                </div>
                <div id="dnnModuleDigitalAssetsCreateFolderMessage" class="dnnFormMessage dnnFormError" style="display: none; margin-bottom: 7px; padding: 10px;"></div>
            </fieldset>
        </div>
    </div>
    
    <div id="dnnModuleDigitalAssetsUploadFileModal" class="dnnModuleDigitalAssetsUploadFileScope" style="display: none; width: 100%;">
        
        <div id="dnnModuleDigitalAssetsUploadFileMessage" class="dnnFormMessage dnnFormError" style="display: none;"></div>
        
        <p id="dnnModuleDigitalAssetsUploadFileInfo">
            <span id="dnnModuleDigitalAssetsUploadFileMultiplesFileInfo"><%=Localization.GetString("UploadFiles.Info", LocalResourceFile) %></span>
            <span id="dnnModuleDigitalAssetsUploadFileDragDropInfo"><%=Localization.GetString("UploadFilesDragDrop.Info", LocalResourceFile) %></span>
        </p>

        <div id="dnnModuleDigitalAssetsUploadFileMain">
            <div id="dnnModuleDigitalAssetsUploadFileDropZone">
                <span><%=Localization.GetString("UploadFiles.DropHere", LocalResourceFile) %></span>
            </div>
            <div id="dnnModuleDigitalAssetsUploadFileExternalResultZone">
                <div id="dnnModuleDigitalAssetsUploadFileResultZone">
                </div>
            </div>
        </div>
        
        <div id="dnnModuleDigitalAssetsUploadFileFooter">
            <a id="dnnModuleDigitalAssetsUploadFileDialogClose" href="#" class="dnnSecondaryAction"><%=Localization.GetString("Close.Text", LocalResourceFile) %></a>
        </div>
    </div>
    <div id="dnnModuleDigitalAssetsAlertItems" style="display: none;">
        <div id="dnnModuleDigitalAssetsAlertItemsSubtext"></div>
        <div id="dnnModuleDigitalAssetsAlertItemsScroll" class="dnnScroll" style="height: 120px; width: 600px">
            <table class="dnnModuleDigitalAssetsAlertItemsTable">
            </table>
        </div>
    </div>
    <div id="dnnModuleDigitalAssetsSelectDestinationFolderModal" style="display: none;">
        <div id="dnnModuleDigitalAssetsDestinationFolderScroll" class="dnnScroll">
            <dnnweb:DnnTreeView ID="DestinationTreeView" runat="server" CssClass="dnnModuleDigitalAssetsDestinationTreeView dnnModuledigitalAssetsTreeView"
                                OnClientNodeExpanding="dnnModule.digitalAssets.destinationTreeViewOnNodeExpanding"
                                OnClientNodeAnimationEnd="dnnModule.digitalAssets.destinationTreeViewRefreshScrollbars"
                                OnClientNodeCollapsed="dnnModule.digitalAssets.destinationTreeViewRefreshScrollbars"
                                OnClientNodeExpanded="dnnModule.digitalAssets.destinationTreeViewRefreshScrollbars"
                                OnClientNodeClicking="dnnModule.digitalAssets.destinationTreeViewOnNodeClicking"
                                OnClientLoad="dnnModule.digitalAssets.destinationTreeViewOnLoad" />
        </div>
    </div>
    
    <div id="dnnModuleDigitalAssetsGetUrlModal" style="display: none;">
        <br />
        <span><%=LocalizeString("GetUrlLabel") %></span>
        <input type="text" readonly="readonly" onclick="this.select()" title="<%=LocalizeString("GetUrlAltText") %>" />
    </div>
    <dnnweb:DnnFileUpload ID="fileUpload" runat="server"/>
</asp:Panel>
<script type="text/javascript">

    dnnModule.digitalAssets.init(
        $.ServicesFramework(<%=ModuleId %>),
        '<%= RootFolderViewModel != null ? RootFolderViewModel.FolderID : 0 %>',
        // Controls
        {
            scopeWrapperId: '<%= ScopeWrapper.ClientID %>',
            treeViewMenuId: '<%= MainContextMenu.ClientID%>',
            gridId: '<%= Grid.ClientID %>',
            gridMenuId: '<%= GridMenu.ClientID %>',
            emptySpaceMenuId: '<%= EmptySpaceMenu.ClientID %>',
            comboBoxFolderTypeId: '<%= FolderTypeComboBox.ClientID %>',
            txtFolderNameId: '<%= FolderNameTextBox.ClientID %>',
            txtMappedPathId: '<%= MappedPathTextBox.ClientID %>',
            mainToolBarId: 'dnnModuleDigitalAssetsMainToolbar',
            selectionToolBarId: 'dnnModuleDigitalAssetsSelectionToolbar',
            fileUploadId: '<%= fileUpload.ClientID %>'
        },
        // Settings
        {
            loadingImageUrl: '<%= ResolveUrl("~/images/dnnanim.gif") %>',
            breadcrumbsImageUrl: '<%= ResolveUrl(IconController.IconURL("BreadcrumbArrows", "16x16", "Gray")) %>',
            toggleLeftPaneHideImageUrl: '<%= ResolveUrl(IconController.IconURL("TreeViewHide", "16x16", "Gray")) %>',
            toggleLeftPaneShowImageUrl: '<%= ResolveUrl(IconController.IconURL("TreeViewShow", "16x16", "Gray")) %>',
            gridViewActiveImageUrl: '<%= ResolveUrl(IconController.IconURL("ListViewActive", "16x16", "Gray")) %>',
            gridViewInactiveImageUrl: '<%= ResolveUrl(IconController.IconURL("ListView", "16x16", "Gray")) %>',
            listViewActiveImageUrl: '<%= ResolveUrl(IconController.IconURL("ThumbViewActive", "16x16", "Gray")) %>',
            listViewInactiveImageUrl: '<%= ResolveUrl(IconController.IconURL("ThumbView", "16x16", "Gray")) %>',
            navigateUrl: '<%= HttpUtility.JavaScriptStringEncode(NavigateUrl)%>',            
            selectedTab: '<%= InitialTab %>',
            isHostMenu: <%= IsHostPortal ? "true" : "false" %>,
            isAuthenticated: <%= Request.IsAuthenticated ? "true" : "false" %>,
            maxFileUploadSize: <%= MaxUploadSize.ToString(CultureInfo.InvariantCulture) %>,
            maxFileUploadSizeHumanReadable: '<%= string.Format(new FileSizeFormatProvider(), "{0:fs}", MaxUploadSize) %>',
            defaultFolderTypeId: '<%= DefaultFolderTypeId %>',
            pageSize: '<%= PageSize %>', 
            view: '<%= ActiveView %>',
            userId: '<%= UserId %>',
            groupId: '<%= Request.Params["GroupId"] %>',
            rootFolderPath: '<%= RootFolderViewModel != null ? RootFolderViewModel.FolderPath : "" %>',
            isFilteredContent: <%= FilteredContent ? "true" : "false" %>
        },
        // Resources
        {
            saveText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("Save")) %>',
            cancelText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("Cancel")) %>',
            fileLabel: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("FileLabel")) %>',
            folderLabel: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("FolderLabel")) %>',
            fileMultLabel: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("FileMultLabel")) %>',
            folderMultLabel: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("FolderMultLabel")) %>',
            createNewFolderTitleText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("CreateNewFolderTitle")) %>',
            loadingAltText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("Loading")) %>',
            uploadErrorText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("UploadError")) %>',
            andText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("And")) %>',
            oneFileText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("OneFile")) %>',
            oneFolderText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("OneFolder")) %>',
            nFilesText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("NFiles")) %>',
            nFoldersText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("NFolders")) %>',
            noItemsDeletedText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("NoItemsDeletedDescription.Text")) %>',
            deleteText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("Delete")) %>',
            deleteTitle: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("Delete.Title")) %>',
            deleteConfirmText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("DeleteConfirm")) %>',
            deleteConfirmWithMappedSubfoldersText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("DeleteConfirmWithMappedSubfolders")) %>',
            deleteConfirmWithMappedSubfolderText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("DeleteConfirmWithMappedSubfolder")) %>',
            okText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("OkConfirm")) %>',
            noText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("NoConfirm")) %>',
            closeText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("Close")) %>',
            extensionChangeConfirmTitleText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("ExtensionChangeConfirmTitle")) %>',
            extensionChangeConfirmContent: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("ExtensionChangeConfirmContent")) %>',
            copyFilesText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("CopyFiles")) %>',
            copyFilesTitle: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("CopyFiles.Title")) %>',
            copyError: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("Copy.Error")) %>',
            noItemsDeletedTitle: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("NoItemsDeleted.Title")) %>',
            moveText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("Move")) %>',
            moveTitle: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("Move.Title")) %>',
            moveError: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("Move.Error")) %>',            
            duplicateFilesExistText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("DuplicateFilesExist.Text")) %>',
            duplicateCopySubtext: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("DuplicateCopy.Subtext")) %>',
            duplicateMoveSubtext: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("DuplicateMove.Subtext")) %>',
            duplicateUploadSubtext: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("DuplicateUpload.Subtext")) %>',
            replaceAllText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("ReplaceAll.Text")) %>',
            keepAllText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("KeepAll.Text")) %>',
            replaceText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("Replace.Text")) %>',
            keepText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("Keep.Text")) %>',
            renameFolderErrorTitle: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("RenameFolderError.Title")) %>',
            createFolderErrorTitle: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("CreateFolderError.Title")) %>',
            renameFileErrorTitle: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("RenameFileError.Title")) %>',
            loadSubFoldersErrorTitle: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("LoadSubFoldersError.Title")) %>',
            loadFolderContentErrorTitle: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("LoadFolderContentError.Title")) %>',
            deleteItemsErrorTitle: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("DeleteItemsError.Title")) %>',
            uploadFilesTitle: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("UploadFiles.Title")) %>',
            fileUploadAlreadyExistsText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("FileUploadAlreadyExists.Text")) %>',
            fileUploadStoppedText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("FileUploadStopped.Text")) %>',
            fileUploadErrorOccurredText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("FileUploadErrorOccurred.Error")) %>',
            fileUploadEmptyFileUploadIsNotSupported: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("FileUploadEmptyFileIsNotSupported.Error")) %>',
            zipConfirmationText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("ZipConfirmation.Text")) %>',
            keepCompressedText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("KeepCompressed.Text")) %>',
            expandFileText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("ExpandFile.Text")) %>',
            chooseFileText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("ChooseFiles.Text")) %>',
            invalidChars: '<%= HttpUtility.JavaScriptStringEncode(InvalidCharacters) %>',
            invalidCharsErrorText: '<%= HttpUtility.JavaScriptStringEncode(InvalidCharactersErrorText) %>',
            getUrlTitle: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("GetUrl.Title")) %>',
            getUrlErrorTitle: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("GetUrlError.Title")) %>',
            getUrlLabel: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("GetUrlLabel.Text")) %>',            
            getFileUrlTitle: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("GetFileUrl.Title")) %>',     
            searchBreadcrumb: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("SearchBreadcrumb.Text")) %>',
            moving: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("Moving.Text")) %>',
            selectAll: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("SelectAll.Text")) %>',
            unselectAll: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("UnselectAll.Text")) %>',
            defaultFolderProviderValues: '<%= HttpUtility.JavaScriptStringEncode(string.Join(",", DefaultFolderProviderValues)) %>',
            firstPageText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("PagerFirstPage.Text")) %>',
            lastPageText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("PagerLastPage.Text")) %>',
            nextPageText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("PagerNextPage.Text")) %>',
            previousPageText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("PagerPreviousPage.Text")) %>',
            pagerTextFormatMultiplePagesText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("PagerTextFormatMultiplePages.Text")) %>',
            pagerTextFormatOnePageText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("PagerTextFormatOnePage.Text")) %>',
            pagerTextFormatOnePageOneItemText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("PagerTextFormatOnePageOneItem.Text")) %>',
            maxFileUploadSizeErrorText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("MaxFileUploadSizeError.Text")) %>',
            unzipFileErrorTitle: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("UnzipFileErrorTitle.Text")) %>',
            uploadingExtracting: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("UploadingExtracting.Text")) %>',
            noItemsText: '<%= HttpUtility.JavaScriptStringEncode("NoItems", LocalResourceFile) %>',
            noItemsSearchText: '<%= HttpUtility.JavaScriptStringEncode("NoItemsSearch", LocalResourceFile) %>',
            unzipFilePromptTitle: '<%= HttpUtility.JavaScriptStringEncode("FileUpload.UnzipFilePromptTitle.Text", Localization.SharedResourceFile) %>',
            unzipFileFailedPromptBody: '<%= HttpUtility.JavaScriptStringEncode("FileUpload.UnzipFileFailedPromptBody.Text", Localization.SharedResourceFile) %>',
            unzipFileSuccessPromptBody: '<%= HttpUtility.JavaScriptStringEncode("FileUpload.UnzipFileSuccessPromptBody.Text", Localization.SharedResourceFile) %>',
            unlinkFolderErrorText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("UnlinkFolderError.Title")) %>',
            unlinkTitle: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("Unlink.Title")) %>',
            unlinkConfirmText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("UnlinkConfirm.Text")) %>',
            unlinkText: '<%= HttpUtility.JavaScriptStringEncode(LocalizeString("Unlink.Text")) %>'
        },
        new dnnModule.DigitalAssetsController($.ServicesFramework(<%=ModuleId %>), {}, {userId: '<%= UserId %>'})
    );
    
</script>