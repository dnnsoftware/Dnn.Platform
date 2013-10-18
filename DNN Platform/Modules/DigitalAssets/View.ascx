<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="View.ascx.cs" Inherits="DotNetNuke.Modules.DigitalAssets.View" %>

<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="DotNetNuke.Modules.DigitalAssets.Components.Controllers" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Import Namespace="DotNetNuke.UI.Utilities" %>
<%@ Import Namespace="DotNetNuke.Entities.Icons" %>

<%@ Register TagPrefix="dnnext" Namespace="DotNetNuke.ExtensionPoints" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnnweb" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
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
                        <dnnweb:DnnTreeView ID="FolderTreeView" runat="server" Skin="Vista" CssClass="dnnModuledigitalAssetsTreeView" 
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
            
            <div id="dnnModuleDigitalAssetsMainToolbar">                
                <dnnext:ToolBarButtonExtensionControl Module="DigitalAssets" runat="server" ID="MainToolBar" Group="Main" />                
            </div>
            <div id="dnnModuleDigitalAssetsSelectionToolbar">
                <span id="dnnModuleDigitalAssetsSelectionText"></span>
                <dnnext:ToolBarButtonExtensionControl Module="DigitalAssets" runat="server" ID="SelectionToolBar" Group="Selection" />
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
                                            <img src="../../images/loading.gif" data-src="#= format(ThumbnailUrl) #" class="#= format(ThumbnailClass) #" alt="#= format(ItemName) #"/>
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
                            OnDataBound="dnnModule.digitalAssets.gridOnDataBound" /> 
                    </ClientSettings>
                    <MasterTableView TableLayout="Fixed" AllowCustomSorting="True" AllowSorting="true" EditMode="InPlace" EnableColumnsViewState="false">
                        <Columns>
                            <dnnweb:DnnGridClientSelectColumn HeaderStyle-Width="44px" UniqueName="Select" />                        
                            <dnnweb:DnnGridBoundColumn UniqueName="ItemName" SortExpression="ItemName" DataField="ItemName" HeaderText="Name"/>
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
            <dnnweb:DnnTreeView ID="DestinationTreeView" runat="server" Skin="Vista" CssClass="dnnModuleDigitalAssetsDestinationTreeView dnnModuledigitalAssetsTreeView"
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
        <%=LocalizeString("GetUrlLabel") %>
        <input type="text" readonly="readonly" onclick="this.select()" title="<%=LocalizeString("GetUrlAltText") %>" />
    </div>

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
            selectionToolBarId: 'dnnModuleDigitalAssetsSelectionToolbar'
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
            navigateUrl: '<%= ClientAPI.GetSafeJSString(NavigateUrl)%>',            
            selectedTab: '0',
            isHostMenu: <%= IsHostMenu ? "true" : "false" %>,
            isAuthenticated: <%= Request.IsAuthenticated ? "true" : "false" %>,
            maxFileUploadSize: <%= MaxUploadSize.ToString(CultureInfo.InvariantCulture) %>,
            maxFileUploadSizeHumanReadable: '<%= string.Format(new FileSizeFormatProvider(), "{0:fs}", MaxUploadSize) %>',
            defaultFolderTypeId: '<%= DefaultFolderTypeId %>',
            pageSize: '<%= PageSize %>', 
            view: '<%= ActiveView %>',
            userId: '<%= UserId %>',
            rootFolderPath: '<%= RootFolderViewModel != null ? RootFolderViewModel.FolderPath : "" %>'
        },
        // Resources
        {
            saveText: '<%= ClientAPI.GetSafeJSString(LocalizeString("Save")) %>',
            cancelText: '<%= ClientAPI.GetSafeJSString(LocalizeString("Cancel")) %>',
            createNewFolderTitleText: '<%= ClientAPI.GetSafeJSString(LocalizeString("CreateNewFolderTitle")) %>',
            loadingAltText: '<%= ClientAPI.GetSafeJSString(LocalizeString("Loading")) %>',
            uploadErrorText: '<%= ClientAPI.GetSafeJSString(LocalizeString("UploadError")) %>',
            andText: '<%= ClientAPI.GetSafeJSString(LocalizeString("And")) %>',
            oneFileText: '<%= ClientAPI.GetSafeJSString(LocalizeString("OneFile")) %>',
            oneFolderText: '<%= ClientAPI.GetSafeJSString(LocalizeString("OneFolder")) %>',
            nFilesText: '<%= ClientAPI.GetSafeJSString(LocalizeString("NFiles")) %>',
            nFoldersText: '<%= ClientAPI.GetSafeJSString(LocalizeString("NFolders")) %>',
            noItemsDeletedText: '<%= ClientAPI.GetSafeJSString(LocalizeString("NoItemsDeletedDescription.Text")) %>',
            deleteText: '<%= ClientAPI.GetSafeJSString(LocalizeString("Delete")) %>',
            deleteTitle: '<%= ClientAPI.GetSafeJSString(LocalizeString("Delete.Title")) %>',
            deleteConfirmText: '<%= ClientAPI.GetSafeJSString(LocalizeString("DeleteConfirm")) %>',
            okText: '<%= ClientAPI.GetSafeJSString(LocalizeString("OkConfirm")) %>',
            noText: '<%= ClientAPI.GetSafeJSString(LocalizeString("NoConfirm")) %>',
            closeText: '<%= ClientAPI.GetSafeJSString(LocalizeString("Close")) %>',
            extensionChangeConfirmTitleText: '<%= ClientAPI.GetSafeJSString(LocalizeString("ExtensionChangeConfirmTitle")) %>',
            extensionChangeConfirmContent: '<%= ClientAPI.GetSafeJSString(LocalizeString("ExtensionChangeConfirmContent")) %>',
            copyFilesText: '<%= ClientAPI.GetSafeJSString(LocalizeString("CopyFiles")) %>',
            copyFilesTitle: '<%= ClientAPI.GetSafeJSString(LocalizeString("CopyFiles.Title")) %>',
            copyError: '<%= ClientAPI.GetSafeJSString(LocalizeString("Copy.Error")) %>',
            noItemsDeletedTitle: '<%= ClientAPI.GetSafeJSString(LocalizeString("NoItemsDeleted.Title")) %>',
            moveText: '<%= ClientAPI.GetSafeJSString(LocalizeString("Move")) %>',
            moveTitle: '<%= ClientAPI.GetSafeJSString(LocalizeString("Move.Title")) %>',
            moveError: '<%= ClientAPI.GetSafeJSString(LocalizeString("Move.Error")) %>',            
            duplicateFilesExistText: '<%= ClientAPI.GetSafeJSString(LocalizeString("DuplicateFilesExist.Text")) %>',
            duplicateCopySubtext: '<%= ClientAPI.GetSafeJSString(LocalizeString("DuplicateCopy.Subtext")) %>',
            duplicateMoveSubtext: '<%= ClientAPI.GetSafeJSString(LocalizeString("DuplicateMove.Subtext")) %>',
            duplicateUploadSubtext: '<%= ClientAPI.GetSafeJSString(LocalizeString("DuplicateUpload.Subtext")) %>',
            replaceAllText: '<%= ClientAPI.GetSafeJSString(LocalizeString("ReplaceAll.Text")) %>',
            keepAllText: '<%= ClientAPI.GetSafeJSString(LocalizeString("KeepAll.Text")) %>',
            replaceText: '<%= ClientAPI.GetSafeJSString(LocalizeString("Replace.Text")) %>',
            keepText: '<%= ClientAPI.GetSafeJSString(LocalizeString("Keep.Text")) %>',
            renameFolderErrorTitle: '<%= ClientAPI.GetSafeJSString(LocalizeString("RenameFolderError.Title")) %>',
            createFolderErrorTitle: '<%= ClientAPI.GetSafeJSString(LocalizeString("CreateFolderError.Title")) %>',
            renameFileErrorTitle: '<%= ClientAPI.GetSafeJSString(LocalizeString("RenameFileError.Title")) %>',
            loadSubFoldersErrorTitle: '<%= ClientAPI.GetSafeJSString(LocalizeString("LoadSubFoldersError.Title")) %>',
            loadFolderContentErrorTitle: '<%= ClientAPI.GetSafeJSString(LocalizeString("LoadFolderContentError.Title")) %>',
            deleteItemsErrorTitle: '<%= ClientAPI.GetSafeJSString(LocalizeString("DeleteItemsError.Title")) %>',
            uploadFilesTitle: '<%= ClientAPI.GetSafeJSString(LocalizeString("UploadFiles.Title")) %>',
            fileUploadAlreadyExistsText: '<%= ClientAPI.GetSafeJSString(LocalizeString("FileUploadAlreadyExists.Text")) %>',
            fileUploadStoppedText: '<%= ClientAPI.GetSafeJSString(LocalizeString("FileUploadStopped.Text")) %>',
            fileUploadErrorOccurredText: '<%= ClientAPI.GetSafeJSString(LocalizeString("FileUploadErrorOccurred.Error")) %>',
            fileUploadEmptyFileUploadIsNotSupported: '<%= ClientAPI.GetSafeJSString(LocalizeString("FileUploadEmptyFileIsNotSupported.Error")) %>',
            zipConfirmationText: '<%= ClientAPI.GetSafeJSString(LocalizeString("ZipConfirmation.Text")) %>',
            keepCompressedText: '<%= ClientAPI.GetSafeJSString(LocalizeString("KeepCompressed.Text")) %>',
            expandFileText: '<%= ClientAPI.GetSafeJSString(LocalizeString("ExpandFile.Text")) %>',
            chooseFileText: '<%= ClientAPI.GetSafeJSString(LocalizeString("ChooseFiles.Text")) %>',
            invalidChars: '<%= ClientAPI.GetSafeJSString(InvalidCharacters) %>',
            invalidCharsErrorText: '<%= ClientAPI.GetSafeJSString(InvalidCharactersErrorText) %>',
            getUrlTitle: '<%= ClientAPI.GetSafeJSString(LocalizeString("GetUrl.Title")) %>',
            getUrlErrorTitle: '<%= ClientAPI.GetSafeJSString(LocalizeString("GetUrlError.Title")) %>',
            searchBreadcrumb: '<%= ClientAPI.GetSafeJSString(LocalizeString("SearchBreadcrumb.Text")) %>',
            moving: '<%= ClientAPI.GetSafeJSString(LocalizeString("Moving.Text")) %>',
            selectAll: '<%= ClientAPI.GetSafeJSString(LocalizeString("SelectAll.Text")) %>',
            unselectAll: '<%= ClientAPI.GetSafeJSString(LocalizeString("UnselectAll.Text")) %>',
            defaultFolderProviderValues: '<%= ClientAPI.GetSafeJSString(string.Join(",", DefaultFolderProviderValues)) %>',
            firstPageText: '<%= ClientAPI.GetSafeJSString(LocalizeString("PagerFirstPage.Text")) %>',
            lastPageText: '<%= ClientAPI.GetSafeJSString(LocalizeString("PagerLastPage.Text")) %>',
            nextPageText: '<%= ClientAPI.GetSafeJSString(LocalizeString("PagerNextPage.Text")) %>',
            previousPageText: '<%= ClientAPI.GetSafeJSString(LocalizeString("PagerPreviousPage.Text")) %>',
            pagerTextFormatMultiplePagesText: '<%= ClientAPI.GetSafeJSString(LocalizeString("PagerTextFormatMultiplePages.Text")) %>',
            pagerTextFormatOnePageText: '<%= ClientAPI.GetSafeJSString(LocalizeString("PagerTextFormatOnePage.Text")) %>',
            pagerTextFormatOnePageOneItemText: '<%= ClientAPI.GetSafeJSString(LocalizeString("PagerTextFormatOnePageOneItem.Text")) %>',
            maxFileUploadSizeErrorText: '<%= ClientAPI.GetSafeJSString(LocalizeString("MaxFileUploadSizeError.Text")) %>',
            unzipFileErrorTitle: '<%= ClientAPI.GetSafeJSString(LocalizeString("UnzipFileErrorTitle.Text")) %>',
            uploadingExtracting: '<%= ClientAPI.GetSafeJSString(LocalizeString("UploadingExtracting.Text")) %>',
            noItemsText: '<%=Localization.GetString("NoItems", LocalResourceFile)%>',
            noItemsSearchText: '<%=Localization.GetString("NoItemsSearch", LocalResourceFile)%>'
        },
        new dnnModule.DigitalAssetsController($.ServicesFramework(<%=ModuleId %>), {})
    );
    
</script>