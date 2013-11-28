<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="View.ascx.cs" Inherits="DotNetNuke.Modules.DigitalAssets.View" %>

<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="DotNetNuke.Services.FileSystem" %>
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
                            OnDataBound="dnnModule.digitalAssets.gridOnDataBound"
                            OnColumnHidden="dnnModule.digitalAssets.gridOnColumnHidden" /> 
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
        <span><%=LocalizeString("GetFileUrlLabel") %></span>
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
            navigateUrl: '<%= Localization.GetSafeJSString(NavigateUrl)%>',            
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
            saveText: '<%= Localization.GetSafeJSString(LocalizeString("Save")) %>',
            cancelText: '<%= Localization.GetSafeJSString(LocalizeString("Cancel")) %>',
            createNewFolderTitleText: '<%= Localization.GetSafeJSString(LocalizeString("CreateNewFolderTitle")) %>',
            loadingAltText: '<%= Localization.GetSafeJSString(LocalizeString("Loading")) %>',
            uploadErrorText: '<%= Localization.GetSafeJSString(LocalizeString("UploadError")) %>',
            andText: '<%= Localization.GetSafeJSString(LocalizeString("And")) %>',
            oneFileText: '<%= Localization.GetSafeJSString(LocalizeString("OneFile")) %>',
            oneFolderText: '<%= Localization.GetSafeJSString(LocalizeString("OneFolder")) %>',
            nFilesText: '<%= Localization.GetSafeJSString(LocalizeString("NFiles")) %>',
            nFoldersText: '<%= Localization.GetSafeJSString(LocalizeString("NFolders")) %>',
            noItemsDeletedText: '<%= Localization.GetSafeJSString(LocalizeString("NoItemsDeletedDescription.Text")) %>',
            deleteText: '<%= Localization.GetSafeJSString(LocalizeString("Delete")) %>',
            deleteTitle: '<%= Localization.GetSafeJSString(LocalizeString("Delete.Title")) %>',
            deleteConfirmText: '<%= Localization.GetSafeJSString(LocalizeString("DeleteConfirm")) %>',
            okText: '<%= Localization.GetSafeJSString(LocalizeString("OkConfirm")) %>',
            noText: '<%= Localization.GetSafeJSString(LocalizeString("NoConfirm")) %>',
            closeText: '<%= Localization.GetSafeJSString(LocalizeString("Close")) %>',
            extensionChangeConfirmTitleText: '<%= Localization.GetSafeJSString(LocalizeString("ExtensionChangeConfirmTitle")) %>',
            extensionChangeConfirmContent: '<%= Localization.GetSafeJSString(LocalizeString("ExtensionChangeConfirmContent")) %>',
            copyFilesText: '<%= Localization.GetSafeJSString(LocalizeString("CopyFiles")) %>',
            copyFilesTitle: '<%= Localization.GetSafeJSString(LocalizeString("CopyFiles.Title")) %>',
            copyError: '<%= Localization.GetSafeJSString(LocalizeString("Copy.Error")) %>',
            noItemsDeletedTitle: '<%= Localization.GetSafeJSString(LocalizeString("NoItemsDeleted.Title")) %>',
            moveText: '<%= Localization.GetSafeJSString(LocalizeString("Move")) %>',
            moveTitle: '<%= Localization.GetSafeJSString(LocalizeString("Move.Title")) %>',
            moveError: '<%= Localization.GetSafeJSString(LocalizeString("Move.Error")) %>',            
            duplicateFilesExistText: '<%= Localization.GetSafeJSString(LocalizeString("DuplicateFilesExist.Text")) %>',
            duplicateCopySubtext: '<%= Localization.GetSafeJSString(LocalizeString("DuplicateCopy.Subtext")) %>',
            duplicateMoveSubtext: '<%= Localization.GetSafeJSString(LocalizeString("DuplicateMove.Subtext")) %>',
            duplicateUploadSubtext: '<%= Localization.GetSafeJSString(LocalizeString("DuplicateUpload.Subtext")) %>',
            replaceAllText: '<%= Localization.GetSafeJSString(LocalizeString("ReplaceAll.Text")) %>',
            keepAllText: '<%= Localization.GetSafeJSString(LocalizeString("KeepAll.Text")) %>',
            replaceText: '<%= Localization.GetSafeJSString(LocalizeString("Replace.Text")) %>',
            keepText: '<%= Localization.GetSafeJSString(LocalizeString("Keep.Text")) %>',
            renameFolderErrorTitle: '<%= Localization.GetSafeJSString(LocalizeString("RenameFolderError.Title")) %>',
            createFolderErrorTitle: '<%= Localization.GetSafeJSString(LocalizeString("CreateFolderError.Title")) %>',
            renameFileErrorTitle: '<%= Localization.GetSafeJSString(LocalizeString("RenameFileError.Title")) %>',
            loadSubFoldersErrorTitle: '<%= Localization.GetSafeJSString(LocalizeString("LoadSubFoldersError.Title")) %>',
            loadFolderContentErrorTitle: '<%= Localization.GetSafeJSString(LocalizeString("LoadFolderContentError.Title")) %>',
            deleteItemsErrorTitle: '<%= Localization.GetSafeJSString(LocalizeString("DeleteItemsError.Title")) %>',
            uploadFilesTitle: '<%= Localization.GetSafeJSString(LocalizeString("UploadFiles.Title")) %>',
            fileUploadAlreadyExistsText: '<%= Localization.GetSafeJSString(LocalizeString("FileUploadAlreadyExists.Text")) %>',
            fileUploadStoppedText: '<%= Localization.GetSafeJSString(LocalizeString("FileUploadStopped.Text")) %>',
            fileUploadErrorOccurredText: '<%= Localization.GetSafeJSString(LocalizeString("FileUploadErrorOccurred.Error")) %>',
            fileUploadEmptyFileUploadIsNotSupported: '<%= Localization.GetSafeJSString(LocalizeString("FileUploadEmptyFileIsNotSupported.Error")) %>',
            zipConfirmationText: '<%= Localization.GetSafeJSString(LocalizeString("ZipConfirmation.Text")) %>',
            keepCompressedText: '<%= Localization.GetSafeJSString(LocalizeString("KeepCompressed.Text")) %>',
            expandFileText: '<%= Localization.GetSafeJSString(LocalizeString("ExpandFile.Text")) %>',
            chooseFileText: '<%= Localization.GetSafeJSString(LocalizeString("ChooseFiles.Text")) %>',
            invalidChars: '<%= Localization.GetSafeJSString(InvalidCharacters) %>',
            invalidCharsErrorText: '<%= Localization.GetSafeJSString(InvalidCharactersErrorText) %>',
            getUrlTitle: '<%= Localization.GetSafeJSString(LocalizeString("GetUrl.Title")) %>',
            getUrlErrorTitle: '<%= Localization.GetSafeJSString(LocalizeString("GetUrlError.Title")) %>',
            getFileUrlLabel: '<%= Localization.GetSafeJSString(LocalizeString("GetFileUrlLabel.Text")) %>',            
            searchBreadcrumb: '<%= Localization.GetSafeJSString(LocalizeString("SearchBreadcrumb.Text")) %>',
            moving: '<%= Localization.GetSafeJSString(LocalizeString("Moving.Text")) %>',
            selectAll: '<%= Localization.GetSafeJSString(LocalizeString("SelectAll.Text")) %>',
            unselectAll: '<%= Localization.GetSafeJSString(LocalizeString("UnselectAll.Text")) %>',
            defaultFolderProviderValues: '<%= Localization.GetSafeJSString(string.Join(",", DefaultFolderProviderValues)) %>',
            firstPageText: '<%= Localization.GetSafeJSString(LocalizeString("PagerFirstPage.Text")) %>',
            lastPageText: '<%= Localization.GetSafeJSString(LocalizeString("PagerLastPage.Text")) %>',
            nextPageText: '<%= Localization.GetSafeJSString(LocalizeString("PagerNextPage.Text")) %>',
            previousPageText: '<%= Localization.GetSafeJSString(LocalizeString("PagerPreviousPage.Text")) %>',
            pagerTextFormatMultiplePagesText: '<%= Localization.GetSafeJSString(LocalizeString("PagerTextFormatMultiplePages.Text")) %>',
            pagerTextFormatOnePageText: '<%= Localization.GetSafeJSString(LocalizeString("PagerTextFormatOnePage.Text")) %>',
            pagerTextFormatOnePageOneItemText: '<%= Localization.GetSafeJSString(LocalizeString("PagerTextFormatOnePageOneItem.Text")) %>',
            maxFileUploadSizeErrorText: '<%= Localization.GetSafeJSString(LocalizeString("MaxFileUploadSizeError.Text")) %>',
            unzipFileErrorTitle: '<%= Localization.GetSafeJSString(LocalizeString("UnzipFileErrorTitle.Text")) %>',
            uploadingExtracting: '<%= Localization.GetSafeJSString(LocalizeString("UploadingExtracting.Text")) %>',
            noItemsText: '<%= Localization.GetSafeJSString("NoItems", LocalResourceFile) %>',
            noItemsSearchText: '<%= Localization.GetSafeJSString("NoItemsSearch", LocalResourceFile) %>'
        },
        new dnnModule.DigitalAssetsController($.ServicesFramework(<%=ModuleId %>), {}, {userId: '<%= UserId %>'})
    );
    
</script>