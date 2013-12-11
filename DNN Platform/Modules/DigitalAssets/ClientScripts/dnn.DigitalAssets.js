// IE8 doesn't like using var dnnModule = dnnModule || {}
if (typeof dnnModule === "undefined" || dnnModule === null) { dnnModule = {}; };

dnnModule.digitalAssets = function ($, $find, $telerik, dnnModal) {

    function init(sf, rootId, controlsSettings, moduleSettings, resourcesSettings, bussinessController) {
        servicesFramework = sf;
        rootFolderId = rootId;
        controls = controlsSettings;
        settings = moduleSettings;
        resources = resourcesSettings;

        controller = bussinessController;

        setupDnnTabs();
        setupDnnMainMenuButtons();

        fileUpload = new dnnModule.DigitalAssetsFileUpload($, sf, moduleSettings, resourcesSettings, refreshFolder, getCurrentFolderPath);
    }

    var fileUpload;

    var searchPattern = "";
    var searchProvider = null;
    
    function setSearchProvider(sp) {
        searchProvider = sp;
        searchProvider.onSearch(function (pattern) {
            var node = getSelectedNode();
            if (!node) {
                node = treeView.findNodeByValue(getRootFolderId());
                node.select();
                $("#dnnModuleDigitalAssetsLeftPaneActions li", '#' + controls.scopeWrapperId).removeClass('selected');
                controller.onLoadFolder();
            }
            currentFolderId = node.get_value();
            searchPattern = pattern;
            loadFolderFirstPage(currentFolderId);
        });
    }
    
    function getSearchProvider() {
        return searchProvider;
    }
    
    function clearSearchPattern() {
        searchPattern = "";
    }

    var gridViewMode = "gridview";
    var listViewMode = "listview";

    var currentView = gridViewMode;

    var controller;

    var servicesFramework;
    var contentServiceUrl = '';

    var rootFolderId;
    function getRootFolderId() {
        return rootFolderId;
    }

    var currentFolderId = -1;
    var currentFolder = null;
    var destinationMove;
    var itemsSelectedToMoveOrCopy;

    var controls;
    var settings;
    var resources;

    var grid;
    var treeView;
    var contextMenu;
    var emptySpaceMenu;
    var destinationTreeView;
    var listView;
    var treeViewContextMenu;
    var refreshMenu;
    var isOnSyncMenuBtn;
    var isOnSyncMenu;
    var gridSelectUnselectAll;

    var currentTab = 0;

    var dragAndDropDistance = 20;
    var dragAndDropDelay = 1000;

    var loadingPanelProcesses = 0;

    var tempBlurEventFunction = "tempBlurEventFunction";

    function setupDnnTabs() {
        // Remove Tab if only exists one Tab
        if ($(".dnnModuleDigitalAssetsTabNav li", '#' + controls.scopeWrapperId).length <= 1) {
            $(".dnnModuleDigitalAssetsTabNav", '#' + controls.scopeWrapperId).remove();
        }

        var options = {};
        var selectedTab = parseInt(settings.selectedTab);
        if (selectedTab != NaN && selectedTab != null) options.selected = selectedTab;
        $('#' + controls.scopeWrapperId)
            .dnnTabs(options)
            .bind("tabsactivate", function (event, ui) {
                currentTab = ui.newTab.index();
                if (currentTab == 0) {
                    treeViewRefreshScrollbars();
                }
            });
    }

    function setupDnnMainMenuButtons() {
        refreshMenu = $('#DigitalAssetsSyncFolderMenuBtnId_wrapper .DigitalAssetsMenuButton_menu');
        refreshMenu.find('.handle').click(function () {
            onOpeningRefreshMenu();
        });

        $('#DigitalAssetsSyncFolderMenuBtnId_wrapper .DigitalAssetsMenuButton_menu ul').hover(function () {
            isOnSyncMenu = true;

        }, function () {
            setTimeout(function () {
                isOnSyncMenu = false;
                if (!isOnSyncMenuBtn) {
                    closeRefreshMenu();
                }
            }, 250);
        });

        refreshMenu.find('.handle').hover(function () {
            isOnSyncMenuBtn = true;
        }, function () {
            setTimeout(function () {
                isOnSyncMenuBtn = false;
                if (!isOnSyncMenu) {
                    closeRefreshMenu();
                }
            }, 250);
        });
    }

    function onOpeningRefreshMenu() {
        refreshMenu = $('#DigitalAssetsSyncFolderMenuBtnId_wrapper .DigitalAssetsMenuButton_menu');
        if (refreshMenu.is(":visible")) {
            closeRefreshMenu();
        } else {
            openRefreshMenu();
        }
    }

    function openRefreshMenu() {
        refreshMenu.show();
        isOnSyncMenuBtn = true;
        $("#DigitalAssetsRefreshFolderBtnId").addClass("expanded");
        $("#DigitalAssetsSyncFolderMenuBtnId").addClass("expanded");
        $('#DigitalAssetsSyncFolderMenuBtnId_wrapper .DigitalAssetsMenuButton_menu ul').focus();
    }

    function closeRefreshMenu() {
        refreshMenu.hide();
        isOnSyncMenu = false;
        $("#DigitalAssetsRefreshFolderBtnId").removeClass("expanded");
        $("#DigitalAssetsSyncFolderMenuBtnId").removeClass("expanded");
    }

    function gridOnGridCreated(sender, eventArgs) {
        $.dnnGridCreated(sender, eventArgs);
        grid = sender.get_masterTableView();
        initGridSelectAllUnselectAll(sender);
        
        // remove PostBack on grid header sorting
        $(".dnnModuleDigitalAssetsGrid > table > thead tr th.rgHeader a").attr("href", "#");
        initializePager();

        var selectedNode = getSelectedNode();
        if (selectedNode == null) {
            return;
        }
        
        currentFolderId = selectedNode.get_value();
        //loadFolderFirstPage(currentFolderId);
        setView(settings.view == listViewMode ? listViewMode : gridViewMode);
        grid.set_pageSize(settings.pageSize ? settings.pageSize : 10);

        controller.gridOnGridCreated(grid);
    }
    
    function initGridSelectAllUnselectAll(sender) {
        var checkColumn = $("#" + controls.gridId + " table thead tr th.rgCheck:first");
        checkColumn.empty();
        gridSelectUnselectAll = $("<input type='checkbox'>");
        checkColumn.append(gridSelectUnselectAll);
        gridSelectUnselectAll.click(function (evt) {
            sender._selectAllRows(grid.get_element().id, "", evt);
        }).dnnCheckbox();
    }

    function gridOnDataBound(sender, eventArgs) {
        refreshPager();
    }

    function initializePager() {
        var pageSizeContainer = $(".dnnModuleDigitalAssetsGrid td.rgPagerCell div.rgAdvPart:first");
        pageSizeContainer.prependTo(pageSizeContainer.parent());
        
        $(".dnnModuleDigitalAssetsGrid input.rgPageFirst:first").val(resources.firstPageText);
        $(".dnnModuleDigitalAssetsGrid input.rgPageLast:first").val(resources.lastPageText);
        $(".dnnModuleDigitalAssetsGrid input.rgPageNext:first").val(resources.nextPageText);
        $(".dnnModuleDigitalAssetsGrid input.rgPagePrev:first").val(resources.previousPageText);
    }

    function refreshPager() {
        var totalItems = grid.get_virtualItemCount();

        if (totalItems == 0) {
            $(".dnnModuleDigitalAssetsGrid .rgWrap.rgArrPart1 input").css('visibility', 'hidden');
            $(".dnnModuleDigitalAssetsGrid .rgWrap.rgNumPart").hide();
            $(".dnnModuleDigitalAssetsGrid .rgWrap.rgArrPart2").hide();
            $(".dnnModuleDigitalAssetsGrid .rgWrap.rgInfoPart").hide();
            return;
        }

        $(".dnnModuleDigitalAssetsGrid .rgWrap.rgArrPart1 input").css('visibility', 'visible');
        $(".dnnModuleDigitalAssetsGrid .rgWrap.rgNumPart").show();
        $(".dnnModuleDigitalAssetsGrid .rgWrap.rgArrPart2").show();
        $(".dnnModuleDigitalAssetsGrid .rgWrap.rgInfoPart").show();

        var currentPage = grid.get_currentPageIndex() + 1;
        var totalPages = grid.get_pageCount();

        if (currentPage == 1) {
            $(".dnnModuleDigitalAssetsGrid .rgWrap.rgArrPart1 input").css('visibility', 'hidden');
        }

        if (totalPages == 1) {
            $(".dnnModuleDigitalAssetsGrid .rgWrap.rgNumPart").hide();
            var text = (totalItems == 1) ? resources.pagerTextFormatOnePageOneItemText : resources.pagerTextFormatOnePageText;
            $(".dnnModuleDigitalAssetsGrid .rgWrap.rgInfoPart").html(text.replace("[ITEMS]", totalItems));

        } else {
            $(".dnnModuleDigitalAssetsGrid .rgWrap.rgInfoPart").html(
                resources.pagerTextFormatMultiplePagesText
                    .replace("[PAGES]", totalPages)
                    .replace("[ITEMS]", totalItems));
        }

        if (totalPages == currentPage) {
            $(".dnnModuleDigitalAssetsGrid .rgWrap.rgArrPart2").hide();
        }
    }

    function treeViewOnLoad(sender, eventArgs) {
        treeView = sender;
        var rootNode = treeView.get_nodes().getNode(0);
        if (!rootNode) {
            return;
        }
        
        initDroppableNode(rootNode);
        
        var $ul = $("#dnnModuleDigitalAssetsLeftPaneActions", '#' + controls.scopeWrapperId);        
        
        var actions = controller.getLeftPaneActions(settings);
        for (var i = 0, size = actions.length; i < size; i++) {
            var $li = $("<li></li>")
                .attr('id', actions[i].id)
                .text(actions[i].text);
            var actionMethod = actions[i].method;
            $li.click(function () {
                var node = treeView.get_selectedNode();
                if (node) {
                    node.unselect();
                }

                $(this).addClass('selected');
                actionMethod();
            });
            $ul.append($li);
        }
    }

    function destinationTreeViewOnLoad(sender, eventArgs) {
        destinationTreeView = sender;
    }

    function contextMenuOnLoad(sender) {
        contextMenu = sender;
    }

    function emptySpaceMenuOnLoad(sender) {
        emptySpaceMenu = sender;
    }

    function emptySpaceContextMenu(event) {        
        var permissions = getCurrentNode().get_attributes().getAttribute("permissions");
        var menuSelector = "#" + controls.emptySpaceMenuId + "_detached";
        checkPermissions(menuSelector, permissions, true, true);
        
        if (settings.isFilteredContent === true) {
            hideMenuOptions(menuSelector + " a.rmLink.disabledIfFiltered");
        }

        emptySpaceMenu.show(event);
    }

    function emptySpaceMenuOnItemClicked(sender, args) {
        var node = getCurrentNode();
        switch (args.get_item().get_value()) {
            case "RefreshFolder":
                refreshFolderNode(node);
                break;
            case "NewFolder":
                createNewFolder(node, 'treeview');
                break;
            case "UploadFiles":
                uploadFiles();
                break;
            case "Properties":
                showPropertiesDialog(node.get_value(), true);
                break;
        }
    }
    
    function listViewOnCreated(sender, eventArgs) {
        listView = sender;
    }

    function treeViewContextMenuOnLoad(sender) {
        treeViewContextMenu = sender;
    }

    function toggleLeftPane() {
        var leftPane = $("#dnnModuleDigitalAssetsLeftPane", "#" + controls.scopeWrapperId);
        var contentPane = $("#dnnModuleDigitalAssetsContentPane", "#" + controls.scopeWrapperId);
        var toggleButton = $("#DigitalAssetsToggleLeftPaneBtnId span", "#" + controls.scopeWrapperId);
        var loadingPanel = $(".dnnModuleDigitalAssetsMainLoading", "#" + controls.scopeWrapperId);
        var left;
        
        if (!leftPane.is(":visible")) {
            toggleButton.css("background-image", "url(" + settings.toggleLeftPaneHideImageUrl + ")");
            leftPane.animate({ width: 'toggle' }, 500, treeViewRefreshScrollbars);
            left = 220;
        } else {
            toggleButton.css("background-image", "url(" + settings.toggleLeftPaneShowImageUrl + ")");
            leftPane.animate({ width: 'toggle' }, 500);
            left = 0;
        }
        
        contentPane.animate({ 'margin-left': left }, 500, 'swing', moreItemsHint);
        loadingPanel.css({ 'left': left });
    }

    function moreItemsHint() {
        if (!$("#dnnModuleDigitalAssetsListView", "#" + controls.scopeWrapperId).is(":visible")) {
            return;
        }

        var numberOfItems = $("#dnnModuleDigitalAssetsListView .dnnModuleDigitalAssetsListViewItem", "#" + controls.scopeWrapperId).length;
        var widthOfItem = $("#dnnModuleDigitalAssetsListView .dnnModuleDigitalAssetsListViewItem", "#" + controls.scopeWrapperId).first().outerWidth();
        var widthOfContainer = $("#dnnModuleDigitalAssetsListView", "#" + controls.scopeWrapperId).width();
        var itemsPerRow = Math.floor(widthOfContainer / widthOfItem);
        if (numberOfItems % itemsPerRow != 0) {
            $("#dnnModuleDigitalAssetsListView", "#" + controls.scopeWrapperId).addClass("moreItems");
        } else {
            $("#dnnModuleDigitalAssetsListView", "#" + controls.scopeWrapperId).removeClass("moreItems");
        }
    }

    function showAlertDialog(title, message, closeAction) {
        if (!message) return;

        $.dnnAlert({
            okText: resources.closeText,
            title: title,
            text: message,
            close: closeAction
        });
    }
    
    function getCurrentFolderId() {
        return currentFolderId;
    }

    function setCurrentFolder(folder) {
        currentFolder = folder;
    }

    function enableLoadingPanel(enable) {
        var loadingPanel = $(".dnnModuleDigitalAssetsMainLoading", "#" + controls.scopeWrapperId);
        if (enable) {
            loadingPanelProcesses = loadingPanelProcesses + 1;
            loadingPanel.show();
        } else {
            loadingPanelProcesses = loadingPanelProcesses - 1;
            if (loadingPanelProcesses <= 0) {
                loadingPanel.hide();
            }
        }
    }

    function showPropertiesFromAction() {
        var folderId = getCurrentNode().get_value();
        showPropertiesDialog(folderId, true);
    }

    function showPropertiesDialog(itemId, isFolder) {
        if (isFolder) {
            showDialog('FolderProperties', { folderId: itemId, groupId: settings.groupId }, 950, 550);
        } else {
            showDialog('FileProperties', { fileId: itemId }, 950, 550);
        }
    }

    function showDialog(controlKey, params, width, height) {
        var url = settings.navigateUrl.replace('ControlKey', controlKey);
        params.skinSrc = 'Portals/_default/Skins/_default/popUpSkin';
        for (var p in params) {
            var charSep = url.indexOf('?') != -1 ? '&' : '?';
            url += charSep + p + "=" + encodeURIComponent(params[p]);
        }
        $("#iPopUp").dialog('option', 'title', '');
        dnnModal.show(url, /*showReturn*/true, height, width, false, null);
    }

    function closeDialog(refresh) {
        if (refresh) {
            internalRefreshNode(getCurrentNode(), false);
            loadFolderCurrentPage(currentFolderId);
        }

        dnnModal.closePopUp(false);
    }

    function getContentServiceUrl() {
        if (contentServiceUrl == '') {
            contentServiceUrl = servicesFramework.getServiceRoot('DigitalAssets') + 'ContentService/';
        }

        return contentServiceUrl;
    }

    function destinationTreeViewOnNodeExpanding(sender, args) {
        internalOnNodeExpanding(args.get_node(), false);
    }

    function destinationTreeViewRefreshScrollbars() {
        $("#dnnModuleDigitalAssetsDestinationFolderScroll").jScrollPane();
    }

    function destinationTreeViewOnNodeClicking(sender, args) {
        var node = args.get_node();
        checkDestinationButtonState(itemsSelectedToMoveOrCopy, node);
    }

    function isSubNode(parentNodeId, node) {
        if (parentNodeId == node.get_value()) {
            return true;
        }
        var parentFolder = node.get_parent();

        if (parentFolder.get_value) {
            return isSubNode(parentNodeId, parentFolder);
        }

        return false;
    }

    function checkDestinationButtonState(selectedItems, node) {
        $("#destination_button").button(checkDestinationPermissions(selectedItems, node) ? 'enable' : 'disable');
    }

    function checkDestinationPermissions(selectedItems, node) {

        var folderId = node.get_value();

        for (var i = 0; i < selectedItems.length; i++) {
            var item = selectedItems[i];

            if (!item.IsFolder) {
                // Files cannot be moved to the same Folder
                if (destinationMove && folderId == item.ParentFolderId) {
                    return false;
                }
            } else {
                // Folder cannot be copied or moved to its Parent Folder
                if (folderId == item.ParentFolderId) {
                    return false;
                }

                // Folder cannot be copied or moved to the same Folder or nested folders
                if (isSubNode(item.ItemId, node)) {
                    return false;
                }
            }
        }

        return nodeHasPermission(node, "ADD");
    }

    function nodeHasPermission(node, permissionKey) {
        var permissionsAttribute = node.get_attributes().getAttribute("permissions");
        var permissions = (typeof permissionsAttribute == "string") ? JSON.parse(permissionsAttribute) : permissionsAttribute;

        return checkSinglePermission(permissions, permissionKey);
    }

    function treeViewOnNodeExpanding(sender, args) {
        $("#dnnModuleDigitalAssetsLeftPaneActions", "#" + controls.scopeWrapperId).hide();
        internalOnNodeExpanding(args.get_node(), true);
    }
    
    function treeViewOnNodeCollapsing() {
        $("#dnnModuleDigitalAssetsLeftPaneActions", "#" + controls.scopeWrapperId).hide();
    }

    function internalOnNodeExpanding(node, isMainTree) {
        if (node.get_expandMode() == Telerik.Web.UI.TreeNodeExpandMode.WebService) {
            node.set_expandMode(Telerik.Web.UI.TreeNodeExpandMode.ClientSide);
            node.showLoadingStatus('<img src="' + settings.loadingImageUrl + '" alt="' + resources.loadingAltText + '"/>', Telerik.Web.UI.TreeViewLoadingStatusPosition.BelowNodeText);
            loadSubFolders(node, false, isMainTree);
        } else {
            if (isMainTree && currentFolderId != node.get_value()) {
                selectSubFolder(node, currentFolderId);
            }
        }
    }

    function treeViewContextMenuOnHiding(sender, args) {
        $(args.get_targetElement()).closest('li.rtLI.selected').removeClass("selected");
    }

    function treeViewOnContextMenuShowing(sender, args) {
        var node = args.get_node();
        $(node.get_element()).addClass('selected');
        var menuSelector = "#" + controls.treeViewMenuId + "_detached";
        $(menuSelector + " li.rmItem").css("display", "");

        controller.setupTreeViewContextMenuExtension(treeViewContextMenu, node);

        if (settings.isFilteredContent === true) {
            hideMenuOptions(menuSelector + " a.rmLink.disabledIfFiltered");
        }

        var permissions = node.get_attributes().getAttribute("permissions");
        checkPermissions(menuSelector, permissions, true, true);
    }

    function treeViewOnContextMenuItemClicking(sender, args) {
        var menuItem = args.get_menuItem();
        var node = args.get_node();
        menuItem.get_menu().hide();

        switch (menuItem.get_value()) {
            case "RefreshFolder":
                refreshFolderNode(node);
                break;
            case "RenameFolder":
                startRenameFolderNode(node);
                break;
            case "NewFolder":
                createNewFolder(node, 'treeview');
                break;
            case "DeleteFolder":
                deleteItems([{
                    ItemId: node.get_value(),
                    IsFolder: true
                }], node.get_parent().get_value());
                break;
            case "Move":
                moveDialog([{
                    ItemId: node.get_value(),
                    IsFolder: true,
                    ParentFolderId: node.get_parent().get_value()
                }]);
                break;
            case "Properties":
                showPropertiesDialog(node.get_value(), true);
                break;
            default:
                controller.executeCommandOnSelectedNode(menuItem.get_value(), node);
                break;
        }
    }

    function cleanItemName(name, isFolder) {

        if (isFolder) {
            name = name.replace(/(\s|\.)+$/, '');
        } else if (name.replace(/(\s|\.)+$/, '') == "") {
            return "";
        }

        return $.trim(name);
    }

    function treeViewOnNodeEditing(sender, args) {
        var node = args.get_node();
        currentFolderId = node.get_value();
        var newText = cleanItemName(args.get_newText(), true);
        args.set_cancel(!renameFolderNode(node, newText));
    }
    
    function expandNode(node) {
        if (node.get_expandMode() == Telerik.Web.UI.TreeNodeExpandMode.WebService) {
            node.set_expandMode(Telerik.Web.UI.TreeNodeExpandMode.ClientSide);
        }
        node.set_expanded(true);
    }

    function startRenameFolderNode(node) {
        node.startEdit();
    }
    
    function internalRefreshNode(node, isAfterRecursiveSych, sender) {
        if (node != null) {
            currentFolderId = node.get_value();
            loadSubFolders(node, isAfterRecursiveSych, true);
            if (sender && sender == 'treeview') {
                if (!node.get_expanded()) {
                    expandNode(node);
                }
            }
        }
    }

    function refreshFolderNode(node, sender) {
        internalRefreshNode(node, false, sender);
        loadFolderFirstPage(currentFolderId);
    }

    function refreshFolder(keepCurrentPage) {
        if (keepCurrentPage && keepCurrentPage === true) {
            internalResetGridComponents();
            var pageSize = grid.get_pageSize();
            var startIndex = grid.get_currentPageIndex() * pageSize;
            loadFolder(currentFolderId, startIndex, pageSize, null);
            return;
        }

        refreshFolderNode(getCurrentNode());
    }
    
    function refreshFolderAfterRecursiveSync() {
        internalRefreshNode(getCurrentNode(), true);
        clearSearchPattern();
        loadFolderFirstPage(currentFolderId);
    }

    function refresFolderFromMenu() {
        onOpeningRefreshMenu();
        refreshFolder();
    }

    function syncFromMenu(recursive) {
        onOpeningRefreshMenu();
        syncFolder(currentFolderId, recursive);
    }

    function syncFolder(folderId, recursive) {

        enableLoadingPanel(true);
        $.ajax({
            url: getContentServiceUrl() + "SynchronizeFolder",
            data: {
                "folderId": folderId,
                "recursive": recursive
            },
            type: "POST",
            beforeSend: servicesFramework.setModuleHeaders
        }).done(function (data) {
            if (recursive) {
                refreshFolderAfterRecursiveSync();
            } else {
                refreshFolder();
            }
        }).fail(function (xhr) {
            handledXhrError(xhr, resources.loadFolderContentErrorTitle);
        }).always(function () {
            enableLoadingPanel(false);
        });
    }

    function getCurrentNode() {
        return treeView.findNodeByValue(currentFolderId);
    }

    function getSelectedNode() {
        return treeView.get_selectedNode();
    }

    function treeViewOnNodeClicking(sender, args) {
        var node = args.get_node();
        currentFolderId = node.get_value();
        $("#dnnModuleDigitalAssetsLeftPaneActions li", '#' + controls.scopeWrapperId).removeClass('selected');
        controller.onLoadFolder();
        clearSearchPattern();
        loadFolderFirstPage(currentFolderId);
    }

    function setNodeText(node, text) {
        treeView.trackChanges();
        node.set_text(text);
        treeView.commitChanges();
    }

    function onRenameFolderNameFail(dialogTitle, dialogText, node, oldText, newText) {
        showAlertDialog(dialogTitle, dialogText, function () {

            // Set the previous -correct- value
            setNodeText(node, oldText);

            // The node will be in Tree mode
            node.startEdit();

            // Pre-select the new -incorrect- value. This is useful to fix the error
            var textInput = node.get_inputElement();
            textInput.value = newText;
        });
    }

    function containsInvalidChars(textValue) {
        var invalidChars = resources.invalidChars;
        for (var i = 0; i < invalidChars.length; i++) {
            if (textValue.indexOf(invalidChars[i]) != -1) {
                return true;
            }
        }
        return false;
    }

    function nameTextIsInvalid(newText, validation) {
        validation.isInvalid = containsInvalidChars(newText);
        if (validation.isInvalid) {
            validation.errorMessage = resources.invalidCharsErrorText;
        }
        return validation.isInvalid;
    }

    function renameFolderNode(node, newFolderName) {

        currentFolderId = node.get_value();
        var oldText = node.get_text();
        if (newFolderName === "" || newFolderName == oldText) {
            setNodeText(node, oldText);
            return false;
        }
        var validation = {};
        if (nameTextIsInvalid(newFolderName, validation)) {
            setTimeout(function () { onRenameFolderNameFail(resources.renameFolderErrorTitle, validation.errorMessage, node, oldText, newFolderName); }, 0);
            return true; // The process cannot be cancelled, because the process needs to replace the oldText, and set edit mode again
        }

        enableLoadingPanel(true);

        $.ajax({
            type: 'POST',
            url: getContentServiceUrl() + 'RenameFolder',
            data: {
                folderId: currentFolderId,
                newFolderName: newFolderName
            },
            beforeSend: servicesFramework.setModuleHeaders
        }).done(function (data) {

            setNodeText(node, newFolderName);

            //Refresh content
            var selectedNode = getSelectedNode();
            if (selectedNode != null && selectedNode.get_value() == node.get_parent().get_value()) {
                var selectedFolderId = selectedNode.get_value();                
                loadFolderFirstPage(selectedFolderId);
            } else { //Refresh only the breadcrumb
                updateBreadcrumb(searchPattern);                
                controller.updateModuleState(createInternalModuleState());
            }
        }).fail(function (xhr, status, error) {
            if (!isXhrHandled(xhr)) {
                showAlertDialog(resources.renameFolderErrorTitle, getExceptionMessage(xhr), function () {

                    // Set the previous -correct- value
                    setNodeText(node, oldText);

                    // The node will be in Tree mode
                    node.startEdit();

                    // Pre-select the new -incorrect- value. This is useful to fix the error
                    var textInput = node.get_inputElement();
                    textInput.value = newFolderName;
                });
            }

        }).always(function () {
            enableLoadingPanel(false);
        });

        return true;
    }

    function getExtension(fileName) {
        if (fileName.indexOf(".") == -1) {
            return "";
        }

        return fileName.split(".").pop().toLowerCase();
    }

    function extensionChanged(oldName, newName) {
        var oldExtension = getExtension(oldName);
        var newExtension = getExtension(newName);
        return oldExtension != newExtension;
    }

    function extensionChangeConfirmation(oldText, newText, renameFunction, cancelFunction) {
        if (!extensionChanged(oldText, newText)) {
            renameFunction();
        } else {
            if ($(".dnnDialog").data("ui-dialog") && $(".dnnDialog").dialog("isOpen")) {
                return;
            }
            var dialogTitle = resources.extensionChangeConfirmTitleText;
            var dialogText = resources.extensionChangeConfirmContent;
            $("<div class='dnnDialog'></div>").html(dialogText).dialog({
                modal: true,
                autoOpen: true,
                dialogClass: "dnnFormPopup",
                width: 400,
                height: 200,
                resizable: false,
                title: dialogTitle,
                buttons:
                    [
                        {
                            id: "ok_button",
                            text: resources.okText,
                            "class": "dnnPrimaryAction",
                            click: function () {
                                renameFunction();
                                $(this).dialog("close");
                            }
                        },
                        {
                            id: "no_button",
                            text: resources.noText,
                            click: function () {
                                $(this).dialog("close");
                            },
                            "class": "dnnSecondaryAction"
                        }
                    ],
                close: function() {
                    cancelFunction();
                }
            });
        }
    }

    function renameItem(dataItem, newText, gridItem, rowId) {
        var requestUrl = getContentServiceUrl() + (dataItem.IsFolder ? 'RenameFolder' : 'RenameFile');
        var requestData = dataItem.IsFolder ?
                            { folderId: dataItem.ItemID, newFolderName: newText } : { fileId: dataItem.ItemID, newFileName: newText };

        enableLoadingPanel(true);
        $.ajax({
            type: 'POST',
            url: requestUrl,
            data: requestData,
            beforeSend: servicesFramework.setModuleHeaders
        }).done(function (data) {
            hideItemEdition(grid.get_selectedItems()[0]);
            dataItem.ItemName = newText;
            dataItem.IconUrl = data.IconUrl;
            dataItem.ThumbnailAvailable = data.ThumbnailAvailable;
            dataItem.ThumbnailUrl = data.ThumbnailUrl;
            finishRename(dataItem);
        }).fail(function (xhr, status, error) {
            if (!isXhrHandled(xhr)) {
                onRenameItemError($("#" + rowId + "_ItemNameEdit"), dataItem.IsFolder, getExceptionMessage(xhr));
            }
        }).always(function () {
            enableLoadingPanel(false);
        });
    }

    function finishRename(dataItem) {
        var rowId = grid.get_selectedItems()[0].get_id();

        // Update name on Grid
        $("#" + rowId + "_ItemName").text(dataItem.ItemName);
        $("#" + rowId + "_ItemNameEdit").val(dataItem.ItemName);

        // Update name on ListView
        var item = $(".dnnModuleDigitalAssetsListViewItem[data-index='" + grid.get_selectedItems()[0].get_itemIndexHierarchical() + "']");
        item.find("span.dnnModuleDigitalAssetsListViewItemLinkName").text(getReducedItemName(dataItem.ItemName));
        item.find("span.dnnModuleDigitalAssetsListViewItemLinkName").attr("title", dataItem.ItemName);

        //Refresh tree view
        if (dataItem.IsFolder) {
            var node = getCurrentNode();
            loadSubFolders(node, false, true);
        } else { // Update icons            
            $("#" + rowId + "_ItemIcon").attr("src", dataItem.IconUrl);
            item.find("div.dnnModuleDigitalAssetsThumbnail>img")
                .attr("src", controller.getThumbnailUrl(dataItem))
                .attr("class", controller.getThumbnailClass(dataItem));
        }
    }

    function renameItemInListView(dataItem, item, input, span, newText) {
        var requestUrl = getContentServiceUrl() + (dataItem.IsFolder ? 'RenameFolder' : 'RenameFile');
        var requestData = dataItem.IsFolder ?
                            { folderId: dataItem.ItemID, newFolderName: newText } : { fileId: dataItem.ItemID, newFileName: newText };

        enableLoadingPanel(true);
        $.ajax({
            type: 'POST',
            url: requestUrl,
            data: requestData,
            beforeSend: servicesFramework.setModuleHeaders
        }).done(function (data) {
            span.show();
            input.remove();
            dataItem.ItemName = newText;
            dataItem.IconUrl = data.IconUrl;
            dataItem.ThumbnailAvailable = data.ThumbnailAvailable;
            dataItem.ThumbnailUrl = data.ThumbnailUrl;
            finishRename(dataItem);
        }).fail(function (xhr) {
            if (!isXhrHandled(xhr)) {
                onRenameItemError(input, dataItem.IsFolder, getExceptionMessage(xhr));
            }
        }).always(function () {
            enableLoadingPanel(false);
        });
    }

    function onRenameItemError(input, isFolder, errorMessage) {
        
        input.off('blur');
        var errorTitle = isFolder ? resources.renameFolderErrorTitle : resources.renameFileErrorTitle;
        showAlertDialog(errorTitle, errorMessage, function () {
            reassignBlurToRenameInput(input);
            selectNameWithoutExtension(input[0]);
        });
    }
    
    function reassignBlurToRenameInput(input) {
        var blurEvent = input.data(tempBlurEventFunction);
        if (blurEvent) {
            input.on('blur', blurEvent);
        }
    }

    function renameItemInGrid(rowId) {

        var gridItem = $find(rowId);
        $(gridItem.get_element()).addClass("continueEditing");
        var dataItem = gridItem.get_dataItem();
        var isFolder = dataItem.IsFolder;
        var oldText = $("#" + rowId + "_ItemName").text();
        var newText = cleanItemName($("#" + rowId + "_ItemNameEdit").val(), isFolder);

        if (newText === "" || oldText == newText) {
            cancelRenameInGrid(rowId);
            return;
        }
        var validation = {};
        if (nameTextIsInvalid(newText, validation)) {
            onRenameItemError($("#" + rowId + "_ItemNameEdit"), isFolder, validation.errorMessage);
            return;
        }

        if (isFolder) {
            renameItem(dataItem, newText, gridItem, rowId);
        } else {
            var input = $("#" + rowId + "_ItemNameEdit");
            input.off('blur');
            extensionChangeConfirmation(oldText, newText, function () {
                renameItem(dataItem, newText, gridItem, rowId);
            },
                function () {
                    reassignBlurToRenameInput(input);
                    selectNameWithoutExtension(input[0]);
                });
        }
    }

    function rename() {
        if (grid.get_selectedItems().length == 1) {
            if (currentView == listViewMode) {

                // TODO: Refactor this code and join with renameItemInGrid / renameItemIn

                var item = $(".dnnModuleDigitalAssetsListViewItem[data-index='" + grid.get_selectedItems()[0].get_itemIndexHierarchical() + "']");

                var span = item.find("span");
                //var oldText = span.text();
                span.hide();

                var dataItem = grid.get_selectedItems()[0].get_dataItem();
                var oldText = dataItem.ItemName;

                var input = $("<input />").val(oldText);
                item.append(input);
                selectNameWithoutExtension(input.get(0));

                input.keypress(function (e) {
                    if (e.which == 13) {
                        e.preventDefault();
                        performRenameInListView($(this), dataItem, span, item);
                    } // enter
                });

                input.keyup(function (e) {
                    if (e.keyCode == 27) {
                        cancelRenameInListView($(this), span);
                    } // esc
                });

                input.click(function (e) {
                    e.stopPropagation();
                });

                input.data(tempBlurEventFunction, function () {
                    performRenameInListView($(this), dataItem, span, item);
                });
                input.blur(input.data(tempBlurEventFunction));

            } else if (currentView == gridViewMode) {
                showRowEdition(grid.get_selectedItems()[0]);
            }
        }
    }

    function performRenameInListView(input, dataItem, span, item) {
        var oldText = dataItem.ItemName;
        var newText = cleanItemName(input.val(), dataItem.IsFolder);

        if (newText === "" || newText == oldText) {
            cancelRenameInListView(input, span);
            return;
        }

        var validation = {};
        if (nameTextIsInvalid(newText, validation)) {
            onRenameItemError(input, dataItem.IsFolder, validation.errorMessage);
            return;
        }

        if (dataItem.IsFolder) {
            renameItemInListView(dataItem, item, input, span, newText);
        } else {
            input.off('blur');
            extensionChangeConfirmation(dataItem.ItemName, newText, function () {
                renameItemInListView(dataItem, item, input, span, newText);
            },
                function () {
                    reassignBlurToRenameInput(input);
                    selectNameWithoutExtension(input[0]);
                });
        }
    }

    function cancelRenameInListView(input, span) {
        input.remove();
        span.show();
    }

    function cancelRenameInGrid(rowId) {
        var oldText = $("#" + rowId + "_ItemName").text();
        $("#" + rowId + "_ItemNameEdit").val(oldText);
        var gridItem = $find(rowId);
        hideItemEdition(gridItem);
    }

    function loadSubFolders(node, reset, isMainTree) {
        var folderParentId = node.get_value();
        
        if (isMainTree) {
            enableLoadingPanel(true);
        }
        $.ajax({
            type: 'POST',
            url: getContentServiceUrl() + 'GetSubFolders',
            data: { folderId: folderParentId },
            async: false,
            beforeSend: servicesFramework.setModuleHeaders
        }).done(function (data) {
            if (reset) {
                resetSubFolders(node, data);
            } else {
                updateSubFolders(node, data, isMainTree);
            }
        }).fail(function (xhr) {
            handledXhrError(xhr, resources.loadSubFoldersErrorTitle);
        }).always(function () {
            if (isMainTree) {
                enableLoadingPanel(false);
            }
        });
    }

    function treeViewRefreshScrollbars() {
        var $actions = $("#dnnModuleDigitalAssetsLeftPaneActions", "#" + controls.scopeWrapperId);
        var $scroll = $("#dnnModuleDigitalAssetsLeftPaneFilesTabContentScroll", "#" + controls.scopeWrapperId);
        $scroll.css({ bottom: $actions.outerHeight() }).jScrollPane();
        var y = Math.min($scroll.find("div.jspPane").outerHeight(), $scroll.find("div.jspContainer").outerHeight());
        $actions.fadeIn(200).css({ top: y });
    }

    function selectSubFolder(node, folderId) {
        var nodes = node.get_allNodes();
        for (var i = 0; i < nodes.length; i++) {
            if (nodes[i].get_value() == folderId) {
                nodes[i].select();
                return;
            }
        }
    }

    function initDroppableNode(node) {
        $(node.get_element()).droppable({
            accept: ".dnnModuleDigitalAssetsListViewItem, .dnnModuledigitalAssetsTreeView .rtLI, .dnnModuleDigitalAssetsGrid tr.rgRow, .dnnModuleDigitalAssetsGrid tr.rgAltRow",
            tolerance: "pointer",
            hoverClass: "dropTarget",
            greedy: true,
            drop: function (event, ui) {
                var element = $(event.target);
                var destNode = treeView.findNodeByValue(element.data('FolderId'));

                if (ui.draggable.is(".rtLI")) {
                    var sourceNode = treeView.findNodeByValue(ui.draggable.data("FolderId"));
                    var sourceItems = [{
                        ItemId: sourceNode.get_value(),
                        IsFolder: true,
                        ParentFolderId: sourceNode.get_parent().get_value()
                    }];

                    if (canDropOnNode(sourceNode, destNode)) {
                        moveItems(sourceItems, destNode.get_value());
                    }
                } else if (canDropOnNode(null, destNode)) {
                    moveItems(convertToItemsFromGridItems(grid.get_selectedItems()), destNode.get_value());
                }
            },
            over: function (event, ui) {
                var element = $(event.target);
                var destNode = treeView.findNodeByValue(element.data('FolderId'));

                dndTreeExpandTimeout = setTimeout(function () {
                    if (!destNode.get_expanded()) {
                        internalOnNodeExpanding(destNode, false);
                        destNode.set_expanded(true);
                    }
                }, 1000);
                
                var sourceNode = ui.draggable.is(".rtLI") ? treeView.findNodeByValue(ui.draggable.data("FolderId")) : null;
                if (!canDropOnNode(sourceNode, destNode)) {
                    element.removeClass("dropTarget");
                }
            },
            out: function (event, ui) {
                clearTimeout(dndTreeExpandTimeout);
            }
        }).data("FolderId", node.get_value()).attr("title", node.get_text())
        .draggable({
            distance: dragAndDropDistance,
            delay: dragAndDropDelay,
            helper: function (event, ui) {
                var text = $(event.target).closest('li.rtLI').find('>div>span.rtIn').text();
                return getDragDropHelper(text);
            },
            start: function (event, ui) {
                var folderId = $(event.target).closest('li.rtLI').data('FolderId');
                if (folderId == rootFolderId) return false;

                var sourceNode = treeView.findNodeByValue(folderId);
                if (!nodeHasPermission(sourceNode, "COPY")) return false;

                $("#" + controls.scopeWrapperId).addClass('moving');
                return true;
            },
            stop: function (event, ui) {
                $("#" + controls.scopeWrapperId).removeClass('moving');
            },
            addClasses: false,
            cursor: "move",
            cursorAt: { left: 10, top: 30 },
            appendTo: "#" + controls.scopeWrapperId
        });

        var nodes = node.get_nodes();
        for (var i = 0; i < nodes.get_count() ; i++) {
            initDroppableNode(nodes.getNode(i));
        }
    }

    function resetSubFolders(parentNode, subfolders) {

        var currentTreeView = parentNode.get_treeView();
        currentTreeView.trackChanges();
        
        parentNode.get_nodes().clear();
        parentNode.set_expandMode(Telerik.Web.UI.TreeNodeExpandMode.ClientSide);
        parentNode.hideLoadingStatus();
        
        for (var i = 0; i < subfolders.length; i++) {
            var node = createNewNode(subfolders[i]);
            parentNode.get_nodes().add(node);
            initDroppableNode(node);

            if (node.get_value() == currentFolderId) {
                node.select();
                updateBreadcrumb();
            }
        }
        
        currentTreeView.commitChanges();
    }

    function updateSubFolders(parentNode, subfolders, isMainTree) {
        
        var currentTreeView = parentNode.get_treeView();
        parentNode.hideLoadingStatus();
        parentNode.set_expandMode(Telerik.Web.UI.TreeNodeExpandMode.ClientSide);
        
        currentTreeView.trackChanges();

        // Deletes Nodes
        var currentNodes = parentNode.get_nodes();
        for (var j = 0; j < currentNodes.get_count() ; j++) {
            var currentNode = currentNodes.getNode(j);
            if (getFolderById(subfolders, currentNode.get_value()) == null) {
                parentNode.get_nodes().removeAt(j);
                j--;
            }
        }
        
        // Add or Update Nodes
        for (var i = 0; i < subfolders.length; i++) {
            var item = subfolders[i];
            var node = currentTreeView.findNodeByValue(item.FolderID);
            if (node == null || node === "undefined") {
                node = createNewNode(item);
                parentNode.get_nodes().add(node);
                if (isMainTree) {
                    initDroppableNode(node);
                }
            } else {
                node = updateNode(node, item);
            }

            if (isMainTree && node.get_value() == currentFolderId) {
                node.select();
                updateBreadcrumb();
            }
        }
        
        currentTreeView.commitChanges();
    }
    
    function getFolderById(items, folderId) {
        for (var i = 0; i < items.length; i++) {
            if (items[i].FolderID == folderId) {
                return items[i];
            }
        }
        return null;
    }

    function createNewNode(item) {
        var node = new Telerik.Web.UI.RadTreeNode();
        return updateNode(node, item);
    }
    
    function updateNode(node, item) {
        node.set_text(item.FolderName);
        node.set_value(item.FolderID);
        node.set_category(item.FolderMappingID);
        node.set_imageUrl(item.IconUrl);

        if ((checkSinglePermission(item.Permissions, "BROWSE") || checkSinglePermission(item.Permissions, "READ")) && item.HasChildren) {
            node.set_expandMode(Telerik.Web.UI.TreeNodeExpandMode.WebService);
        } else {
            node.set_expandMode(Telerik.Web.UI.TreeNodeExpandMode.ClientSide);
        }
        node.get_attributes().setAttribute("permissions", item.Permissions);

        for (var i = 0; i < item.Attributes.length; i++) {
            var attribute = item.Attributes[i];
            node.get_attributes().setAttribute(attribute.Key, attribute.Value);
        }

        return node;
    }
    
    function copyNodeSettings(nodeCopyTo, nodeCopyFrom) {
        nodeCopyTo.set_text(nodeCopyFrom.get_text());
        nodeCopyTo.set_value(nodeCopyFrom.get_value());
        nodeCopyTo.set_category(nodeCopyFrom.get_category());
        nodeCopyTo.set_imageUrl(nodeCopyFrom.get_imageUrl());

        // Copy attributes
        var attributesCopyFrom = nodeCopyFrom.get_attributes();
        for (var i = 0; i < attributesCopyFrom.get_count(); i++) {
            var attributeKey = attributesCopyFrom._keys[i];
            nodeCopyTo.get_attributes().setAttribute(attributeKey, attributesCopyFrom.getAttribute(attributeKey));
        }

        nodeCopyTo.set_expandMode(nodeCopyFrom.get_expandMode());
        nodeCopyTo.set_expanded(nodeCopyFrom.get_expanded());
    }

    function cloneNode(node) {
        var clonedNode = new Telerik.Web.UI.RadTreeNode();
        copyNodeSettings(clonedNode, node);
        return clonedNode;
    }
    
    function cloneNodeRecursive(sourceNode, destinationNode) {

        var nodes = sourceNode.get_nodes();
        for (var i = 0; i < nodes.get_count() ; i++) {
            var currentNode = nodes.getNode(i);
            var clonedNode = cloneNode(currentNode);
            destinationNode.get_nodes().add(clonedNode);

            cloneNodeRecursive(currentNode, clonedNode);
        }
    }

    function checkCurrentFolderToolBarPermissions(permissions) {
        checkPermissions("#" + controls.mainToolBarId, permissions, true, false);
    }

    function checkSinglePermission(permissions, permissionKey) {
        for (var i = 0; i < permissions.length; i++) {
            if (permissions[i].Key == permissionKey) {
                return permissions[i].Value;
            }
        }
        return false;
    }

    function checkPermissions(selectorPattern, permissionsAttribute, reset, changeParent) {
        var permissions = (typeof permissionsAttribute == "string") ? JSON.parse(permissionsAttribute) : permissionsAttribute;

        //Permission keys are an OR clausure: If any permission has granted an item, then no one more should deny it
        if (reset) {
            $(selectorPattern + " .permission_denied").removeClass("permission_denied");
        }
        for (var i = 0; i < permissions.length; i++) {
            //Select all menu items that must check the current permission            
            var itemsSelector = selectorPattern + " .permission_" + permissions[i].Key;

            var $item = changeParent ? $(itemsSelector).parent() : $(itemsSelector);

            if (permissions[i].Value) {
                $item.addClass("permission_granted");
            } else {
                if (!$item.hasClass("permission_granted")) {
                    $item.addClass("permission_denied");
                }
            }
        }
        $(selectorPattern + " .permission_granted").removeClass("permission_granted");       
    }

    function checkPermissionsWhenItemSelectionChanged(items, selectorPattern) {
        //Permission keys are an OR clausure: If any permission has granted an item, then no one more should deny it
        $(selectorPattern + " .permission_denied", "#" + controls.scopeWrapperId).removeClass("permission_denied");
        for (var j = 0; j < items.length; j++) {
            var item = items[j].get_dataItem();
            if (item) {
                checkPermissions(selectorPattern, item.Permissions, false, false);
            }
        }
    }

    function checkColumnVisibility() {
        var nameColumn = grid.getColumnByUniqueName('ItemName').get_element();
        var showColumns = nameColumn && $(nameColumn).width() > 100;
        toggleColumn('LastModifiedOnDate', showColumns);
        toggleColumn('Size', showColumns);
    }
    
    function gridOnColumnHidden() {
        $('#' + controls.gridId + '>table', "#" + controls.scopeWrapperId).hide().show(); // FF workaround to hide the space of the last column
    }

    function internalResetGridComponents() {
        toggleColumn('LastModifiedOnDate', true);
        toggleColumn('ParentFolder', false);        
        
        grid.clearSort();

        checkColumnVisibility();

        if (searchProvider && (!searchPattern || searchPattern == '')) {
            searchProvider.clearSearch();
        }
    }

    function loadInitialContent() {
        loadFolderFirstPage(currentFolderId);
    }

    function loadFolderFirstPage(folderId) {
        internalResetGridComponents();
        grid.set_currentPageIndex(0);
        loadFolder(folderId, 0, grid.get_pageSize(), null);
    }
        
    function createInternalModuleState() {        
        return createModuleState("folderId", getCurrentNode().get_value());
    }
    
    function createModuleState(stateMode, stateValue) {
        var state = controller.getCurrentState(grid, currentView);
        state.stateMode = stateMode;
        state.stateValue = stateValue;
        return state;
    }

    function loadFolderCurrentPage(folderId) {
        internalResetGridComponents();
        loadFolder(folderId, grid.get_currentPageIndex(), grid.get_pageSize(), null);
    }
    
    function handledXhrError(xhr, message) {
        if (!isXhrHandled(xhr)) {
            showAlertDialog(message, getExceptionMessage(xhr));
        }
    }

    function loadFolder(folderId, startIndex, numItems, sortExpression) {

        if (controller.loadContent(folderId, startIndex, numItems, sortExpression, settings, controls.scopeWrapperId)) {
            currentFolder = null;
            return;
        }
        
        if (searchProvider && searchPattern && searchPattern != "") {
            prepareForFilteredContent();
            searchProvider.doSearch(folderId, searchPattern, startIndex, numItems, sortExpression,
                function () {
                    enableLoadingPanel(true);
                },
                function (data) {
                    itemsDatabind(data, resources.noItemsSearchText);
                },
                function (xhr, status, error) {
                     handledXhrError(xhr, resources.loadFolderContentErrorTitle);
                },
                function () {
                    enableLoadingPanel(false);
                }
            );
            currentFolder = null;
            return;
        }

        grid.clearSelectedItems();
        enableLoadingPanel(true);        
        $.ajax({
            url: getContentServiceUrl() + "GetFolderContent",
            data: {
                "folderId": folderId,
                "startIndex": startIndex,
                "numItems": numItems,
                "sortExpression": sortExpression
            },
            type: "POST",
            beforeSend: servicesFramework.setModuleHeaders
        }).done(function (data) {
            if (settings.isFilteredContent === true) {
                $('#dnnModuleDigitalAssetsMainToolbar #DigitalAssetsUploadFilesBtnId', "#" + controls.scopeWrapperId).css("display", "");
            }
            currentFolder = data.Folder;
            currentFolder.ItemID = currentFolder.FolderID;
            currentFolder.IsFolder = true;
            checkCurrentFolderToolBarPermissions(data.Folder.Permissions);
            itemsDatabind(data, resources.noItemsText);
        }).fail(function (xhr) {
            handledXhrError(xhr, resources.loadFolderContentErrorTitle);
        }).always(function () {
            enableLoadingPanel(false);                        
            controller.updateModuleState(createInternalModuleState());
        });
    }

    function getParentFolderName(parentFolder) {
        if (parentFolder.indexOf(settings.rootFolderPath) == 0) {
            parentFolder = parentFolder.substring(settings.rootFolderPath.length);
        }
        
        return treeView.get_nodes().getNode(0).get_text() + '/' + parentFolder;
    }

    function itemsDatabind(data, noItemsText) {
        
        for (var i = 0; i < data.Items.length; i++) {
            data.Items[i].ParentFolder = getParentFolderName(data.Items[i].ParentFolder);
        }

        grid.set_virtualItemCount(data.TotalCount);
        grid.set_dataSource(data.Items);
        grid.dataBind();

        $("#" + controls.gridId + " tbody input[type='checkbox']").dnnCheckbox()
            .unbind('click', gridSelectionCheckboxClick)
            .bind('click', gridSelectionCheckboxClick);
        gridSelectUnselectAll.attr("checked", false);

        if (settings.isFilteredContent === false) {
            $(".dnnModuleDigitalAssetsGrid tr.rgRow, .dnnModuleDigitalAssetsGrid tr.rgAltRow").draggable({
                distance: dragAndDropDistance,
                delay: dragAndDropDelay,
                addClasses: false,
                helper: onItemHelper,
                cursor: "move",
                cursorAt: { left: 10, top: 30 },
                appendTo: "#" + controls.scopeWrapperId,
                start: initDragAndDropGridSelection,
                refreshPositions: true
            }).droppable({
                addClasses: false,
                greedy: true,
                hoverClass: "dropTarget",
                accept: ".dnnModuleDigitalAssetsGrid tr.rgRow, .dnnModuleDigitalAssetsGrid tr.rgAltRow, .dnnModuledigitalAssetsTreeView .rtLI",
                over: onItemDragOver,
                drop: onItemDrop
            });
        }

        listView.set_dataSource(prepareListViewData(data));
        listView.dataBind();
        $("#dnnModuleDigitalAssetsListView .dnnModuleDigitalAssetsListViewItem .dnnModuleDigitalAssetsListViewItemLinkName").bind("click", clickOnListViewItemNameLink);        
        $("#dnnModuleDigitalAssetsListViewToolbar input[type=checkbox]", '#' + controls.scopeWrapperId).unbind("click", listviewSelectAllOnClick).bind("click", listviewSelectAllOnClick);
        listViewInitialize();

        if (settings.isFilteredContent === false) {
            $(".dnnModuleDigitalAssetsListViewItem").draggable({
                distance: dragAndDropDistance,
                delay: dragAndDropDelay,
                addClasses: false,
                helper: onItemHelper,
                cursor: "move",
                cursorAt: { left: 10, top: 30 },
                start: initDragAndDropGridSelection,
                refreshPositions: true
            }).droppable(
                {
                    addClasses: false,
                    greedy: true,
                    hoverClass: "dropTarget",
                    accept: ".dnnModuleDigitalAssetsListViewItem, .dnnModuledigitalAssetsTreeView .rtLI",
                    over: onItemDragOver,
                    drop: onItemDrop
                });
        }

        updateSelectionToolBar();
        updateBreadcrumb();

        if (data.Items.length == 0) {
            $('#dnnModuleDigitalAssetsListViewNoItems', '#' + controls.scopeWrapperId).show();
            $('span.dnnModuleDigitalAssetsNoItems', '#' + controls.scopeWrapperId).text(noItemsText);
        } else {
            $('#dnnModuleDigitalAssetsListViewNoItems', '#' + controls.scopeWrapperId).hide();
        }

        treeViewRefreshScrollbars();
    }

    function prepareListViewData(data) {
        if (data.Items) {
            for (var i = 0; i < data.Items.length; i++) {
                data.Items[i].ThumbnailUrl = controller.getThumbnailUrl(data.Items[i]);
                data.Items[i].ThumbnailClass = controller.getThumbnailClass(data.Items[i]);
            }
        }
        return data.Items;
    }

    function getReducedItemName(name, isFolder) {
        if (name.length > 44 || containsLongWords(name)) { //Item Name is greater than 44 characters or contains a long word
            return reduceItemName(name, isFolder);
        } else
            return name;
    }

    function containsLongWords(name) {
        var words = name.split(" ");
        for (var i = 0; i < words.length; i++) {
            if (words[i].length > 22) { // If a word contains more than 22 characters then it is a long word
                return true;
            }
        }
        return false;
    }

    function reduceItemName(name, isFolder) {
        var startName = name.substring(0, 8);
        var endName = isFolder ? name.substring(name.length - 6) //If item is a folder
                                    : name.substring(name.length - (6 + 4)); //If item is a file, extension is included
        return startName + "...." + endName;
    }

    var gridSelectionCheckboxClick = function (event) {        
        event.preventDefault();

        var index = $(this).closest("tr").data("index");
        toggleGridItemSelection(grid.get_dataItems()[index]);
    };

    var toggleGridItemSelection = function (item) {
        if (item.get_selected()) {
            grid.deselectItem(item.get_element());
        } else {
            grid.selectItem(item.get_element());
        }
    };

    function triggerMouseClick(target, ctrlKey, altKey, shiftKey) {

        var fireOnThis = target;
        if (document.createEvent) { // all browsers except IE before version 9
            var evObj = document.createEvent('MouseEvents');
            evObj.initMouseEvent("click", true, true, window,
                0, 0, 0, 0, 0, ctrlKey, altKey, shiftKey, false, 0, null);
            fireOnThis.dispatchEvent(evObj);

        } else if (document.createEventObject) { // IE before version 9
            var evObjIE = document.createEventObject();
            evObjIE.ctrlKey = ctrlKey;
            evObjIE.altKey = altKey;
            evObjIE.shiftKey = shiftKey;
            fireOnThis.fireEvent('onclick', evObjIE);
        }
    }

    function executeViewCommand(sender, commandName) {

        if (commandName == "ClearSort") return;

        var masterView = sender.get_masterTableView();
        var pageSize = masterView.get_pageSize();
        var sortExpressions = masterView.get_sortExpressions();
        var currentPageIndex = masterView.get_currentPageIndex();

        if (commandName == "Sort" || commandName == "Filter") {
            currentPageIndex = 0;
            masterView.set_currentPageIndex(0);
        }
        var sortExpressionsAsSql = sortExpressions.toString();

        loadFolder(currentFolderId, currentPageIndex * pageSize, pageSize, sortExpressionsAsSql);
    }

    function gridOnCommand(sender, args) {
        args.set_cancel(true);
        executeViewCommand(sender, args.get_commandName());
    }

    function getGridDataItemById(id) {
        var dataItems = grid.get_dataItems();

        for (var i = 0; i < dataItems.length; i++) {
            if (dataItems[i].get_dataItem().ItemID == id) {
                return dataItems[i].get_dataItem();
            }
        }
        return null;
    }

    function hideItemEdition(gridItem) {
        var celltd = gridItem.get_cell("ItemName");

        //Show the text
        $("div > span > span", celltd).show();

        //Hide the input
        $("#" + gridItem.get_id() + "_ItemNameEdit").remove();
    }

    function showRowEdition(gridItem) {
        var celltd = gridItem.get_cell("ItemName");

        $("div > span > span", celltd).hide();

        //Show the input
        var rowId = gridItem.get_id();
        var inputItemName = $("<input></input>");
        inputItemName.attr("id", rowId + "_ItemNameEdit");
        inputItemName.addClass("ItemNameEdit");
        inputItemName.val(gridItem.get_dataItem().ItemName);
        inputItemName.keypress(function (e) {
            if (e.which == 13) {
                renameItemInGrid(rowId);

                //Stop propagation. Fix FireFox issue
                e.cancelBubble = true;
                e.returnValue = false;

                if (e.stopPropagation) {
                    e.stopPropagation();
                    e.preventDefault();
                }
            } // enter
        });

        inputItemName.keyup(function (e) {
            if (e.keyCode == 27) {
                cancelRenameInGrid(rowId);
            } // esc
        });

        inputItemName.data(tempBlurEventFunction, function () {
            renameItemInGrid(rowId);
        });
        inputItemName.blur(inputItemName.data(tempBlurEventFunction));
        
        inputItemName.click(function (e) {
            return false;
        });
        
        $("div", celltd).append(inputItemName);
        selectNameWithoutExtension(inputItemName[0]);
    }

    function selectNameWithoutExtension(textBox) {
        var index = textBox.value.lastIndexOf('.');
        if (index == -1) {
            textBox.select();
            return;
        }

        if (textBox.createTextRange) {
            var selRange = textBox.createTextRange();
            selRange.collapse(true);
            selRange.moveStart('character', 0);
            selRange.moveEnd('character', index);
            selRange.select();
        } else if (textBox.setSelectionRange) {
            textBox.setSelectionRange(0, index);
        } else if (textBox.selectionStart) {
            textBox.selectionStart = 0;
            textBox.selectionEnd = index;
        }
        textBox.focus();
    }
    
    function clickOnItemName(event, dataItem) {
        event.preventDefault();
        event.stopPropagation();

        if (dataItem.IsFolder) {
            currentFolderId = dataItem.ItemID;
            var node = getExpandedNodeByPath(dataItem.ParentFolder);
            selectSubFolder(node, dataItem.ItemID);
            $("#dnnModuleDigitalAssetsLeftPaneActions li", '#' + controls.scopeWrapperId).removeClass('selected');            
            controller.onLoadFolder();
            clearSearchPattern();
            loadFolderFirstPage(dataItem.ItemID);
        } else {            
            self.window.open(setTimeStamp(getUrlAsync(dataItem.ItemID)));
        }
    }

    function getExpandedNodeByPath(path) {
        $("#dnnModuleDigitalAssetsLeftPaneActions", "#" + controls.scopeWrapperId).hide();
        var node = treeView.get_nodes().getItem(0);
        node.expand();
        var p = path.split('/');
        for (var i = 0; i < p.length; i++) {
            var name = p[i];
            if (name != '') {
                var nodes = node.get_nodes();
                for (var j = 0; j < nodes.get_count() ; j++) {
                    var n = nodes.getItem(j);
                    if (n.get_text() == name) {
                        node = n;
                        internalOnNodeExpanding(node, true);
                        node.set_expanded(true);
                        break;
                    }
                }
            }
        }

        return node;
    }

    function getUrlAsync(fileId) {
        var url;
        enableLoadingPanel(true);
        $.ajax({
            type: 'POST',
            url: getContentServiceUrl() + 'GetUrl',
            data: {
                fileId: fileId
            },
            async: false,
            beforeSend: servicesFramework.setModuleHeaders
        }).done(function(data) {
            url = data;
        }).fail(function (xhr) {
            handledXhrError(xhr, resources.getUrlErrorTitle);
        }).always(function () {
            enableLoadingPanel(false);
        });
        return url;
    }
    
    function clickOnListViewItemNameLink(event) {

        var index = $(this).parent().attr("data-index");
        var gridItem = grid.get_dataItems()[index];

        if (gridItem) {
            var dataItem = gridItem.get_dataItem();
            clickOnItemName(event, dataItem);
        }
    }

    function highlightItemName(itemName) {
        if (searchProvider) {
            itemName = searchProvider.highlightItemName(searchPattern, itemName);
        }

        return itemName;
    }

    function getItemNameColumnContent(rowId, dataItem) {
        var iconItem = $("<img></img>");
        iconItem.attr("id", rowId + "_ItemIcon");
        iconItem.attr("src", dataItem.IconUrl);
        iconItem.addClass("ItemIcon");

        var spanItemName = $("<span></span>");
        spanItemName.attr("id", rowId + "_ItemName");
        spanItemName.html(highlightItemName(dataItem.ItemName));

        var divItemName = $("<div></div>");
        divItemName.attr("id", rowId + "_ItemNameTemplate");
        divItemName.attr("title", dataItem.ItemName);
        divItemName.addClass("dnnModuleDigitalAssetItemNameTemplate");
        
        var span = $("<span></span>");
        span.append(iconItem);
        span.append(spanItemName);
        span.click(function (event) {
            clickOnItemName(event, dataItem);
        });

        divItemName.append(span);
        
        return divItemName;
    }

    function gridOnRowDataBound(sender, args) {
        var item = args.get_item();

        $(item.get_element()).data("index", item.get_itemIndexHierarchical());

        var selectCell = $(item.get_cell("Select"));
        selectCell.html("<input type='checkbox' />");

        //the firstChild -textNode- will be replaced with a HTMl node designed to be edited
        var cellItemNametd = $(item.get_cell("ItemName"));

        cellItemNametd.empty();
        cellItemNametd.append(getItemNameColumnContent(item.get_id(), args.get_dataItem()));

        var cellSizetd = $(item.get_cell("Size"));
        cellSizetd.addClass("dnnModuleDigitalAssetsGrid-SizeColumn");

        controller.gridOnRowDataBound(item, args.get_dataItem());
    }

    function onContextMenuShown() {
        // RadMenu asigns overflow: hidden to hidden items. This is not hidding the bottom border
        $('.RadMenu.dnnModuleDigitalAssetsContextMenu li').each(function () {
            if ($(this).css("overflow") == "hidden") {
                $(this).hide();
            }
            $(this).removeClass('rmFirst').removeClass('rmLast');
        });

        // Change the rmFirst and rmLast classes considering item visibility
        $('.RadMenu.dnnModuleDigitalAssetsContextMenu').find('li:visible:first').addClass('rmFirst');
        $('.RadMenu.dnnModuleDigitalAssetsContextMenu').find('li:visible:last').addClass('rmLast');
    }

    function isItemAlreadySelected(gridItem, items) {

        var item = gridItem.get_dataItem();
        for (var i = 0; i < items.length; i++) {
            if (items[i].ItemId == item.ItemID) {
                return true;
            }
        }
        return false;
    }

    function setupSelectedItemsToContextMenu(gridItem, ctrlKey, shiftKey) {
        var items = convertToItemsFromGridItems(grid.get_selectedItems());
        if (ctrlKey && !shiftKey) {
            return items;
        }
        if (isItemAlreadySelected(gridItem, items)) {
            return items;
        } else {
            grid.clearSelectedItems();
            grid.selectItem(gridItem.get_element(), true);
            return convertToItemsFromGridItems(grid.get_selectedItems());
        }
    }

    function hideMenuOptions(selector) {
        $(selector).each(function () {
            $(this).parent().hide();
        });
    }

    function setupContextMenu(index, event) {
        var gridItem = grid.get_dataItems()[index];
        var items = setupSelectedItemsToContextMenu(gridItem, event.ctrlKey, event.shiftKey);
        //If there is no items selected, the menu is not show
        if (items.length <= 0) {
            return;
        }

        var menuSelector = "#" + controls.gridMenuId + "_detached";
        $(menuSelector + " li.rmItem").css("display", "");

        if (items.length > 1) {
            hideMenuOptions(menuSelector + " a.rmLink.singleItem");            
        } else {
            hideMenuOptions(menuSelector + " a.rmLink.moreThanOneItem");
        }

        if (!areOnlyFilesSelected(items)) {
            hideMenuOptions(menuSelector + " a.rmLink.onlyFiles");
        }
        
        if (!areOnlyFoldersSelected(items)) {
            hideMenuOptions(menuSelector + " a.rmLink.onlyFolders");
        }

        if (settings.isFilteredContent === true) {
            hideMenuOptions(menuSelector + " a.rmLink.disabledIfFiltered");
        }

        var downloadVisible = controller.isDownloadAvailable(items);
        contextMenu.findItemByValue("Download").set_visible(downloadVisible);

        var unzip = contextMenu.findItemByValue("UnzipFile");
        if (getExtension(gridItem.get_dataItem().ItemName) != "zip") {
            unzip.set_visible(false);
        }

        controller.setupGridContextMenuExtension(contextMenu, grid.get_selectedItems());

        var permissions = gridItem.get_dataItem().Permissions;
        checkPermissions(menuSelector, permissions, true, true);
        
        contextMenu.show(event);
    }

    function gridOnRowContextMenu(sender, args) {

        var evt = args.get_domEvent();
        if (evt.target.tagName == "INPUT" || evt.target.tagName == "A") {
            return;
        }

        setupContextMenu(args.get_itemIndexHierarchical(), evt);

        evt.cancelBubble = true;
        evt.returnValue = false;

        if (evt.stopPropagation) {
            evt.stopPropagation();
            evt.preventDefault();
        }
    }

    function areOnlyFilesSelected(items) {
        for (var i = 0; i < items.length; i++) {
            if (items[i].IsFolder) {
                return false;
            }
        }
        return true;
    }

    function areOnlyFoldersSelected(items) {
        for (var i = 0; i < items.length; i++) {
            if (!items[i].IsFolder) {
                return false;
            }
        }
        return true;
    }

    function updateSelectionToolBar() {

        var items = convertToItemsFromGridItems(grid.get_selectedItems());

        if (items.length == 0) {
            $(".DigitalAssetsSelectionToolBar", "#" + controls.scopeWrapperId).hide();
            $("#dnnModuleDigitalAssetsSelectionText").html("");
            return;
        }

        $(".DigitalAssetsSelectionToolBar", "#" + controls.scopeWrapperId).css("display", "");
        $("#dnnModuleDigitalAssetsSelectionText").html(selectionText(items));

        checkPermissionsWhenItemSelectionChanged(grid.get_selectedItems(), "#" + controls.selectionToolBarId);

        if (items.length > 1) {
            $(".DigitalAssetsSelectionToolBar.singleItem", "#" + controls.scopeWrapperId).hide();
        } else {
            $(".DigitalAssetsSelectionToolBar.moreThanOneItem", "#" + controls.scopeWrapperId).hide();
        }

        if (!areOnlyFilesSelected(items)) {
            $(".DigitalAssetsSelectionToolBar.onlyFiles", "#" + controls.scopeWrapperId).hide();
        }
        
        if (!areOnlyFoldersSelected(items)) {
            $(".DigitalAssetsSelectionToolBar.onlyFolders", "#" + controls.scopeWrapperId).hide();
        }

        if (settings.isFilteredContent === true) {
            $(".DigitalAssetsSelectionToolBar.disabledIfFiltered", "#" + controls.scopeWrapperId).hide();
        }

        if (controller.isDownloadAvailable(items)) {
            $("#DigitalAssetsDownloadBtnId", "#" + controls.scopeWrapperId).css("display", "");
        } else {
            $("#DigitalAssetsDownloadBtnId", "#" + controls.scopeWrapperId).hide();
        }

        if ($("#DigitalAssetsUnzipFileBtnId", "#" + controls.scopeWrapperId).is(":visible")) {
            if (getExtension(grid.get_selectedItems()[0].get_dataItem().ItemName) == "zip") {
                $("#DigitalAssetsUnzipFileBtnId", "#" + controls.scopeWrapperId).css("display", "");
            } else {
                $("#DigitalAssetsUnzipFileBtnId", "#" + controls.scopeWrapperId).hide();
            }
        }

        var $selectionToolbar = $("#dnnModuleDigitalAssetsSelectionToolbar", "#" + controls.scopeWrapperId);
        controller.updateSelectionToolBar($selectionToolbar, grid.get_selectedItems());
    }

    function getBreadcrumbFolderItem(node) {
        var a = $("<a href='#' />")
            .text(node.get_text())
            .attr("title", node.get_text())
            .attr("data-folderid", node.get_value())
            .click(function (e) {
                e.preventDefault();
                currentFolderId = $(this).attr('data-folderid');
                treeView.findNodeByValue(currentFolderId).select();
                clearSearchPattern();
                loadFolderFirstPage(currentFolderId);
            });
        return $("<li class='dnnModuleDigitalAssetsBreadcrumbLink' />").append(a);
    }

    function updateBreadcrumb() {
        var node = treeView.findNodeByValue(currentFolderId);
        var ul = $('#dnnModuleDigitalAssetsBreadcrumb ul');

        if (searchPattern && searchPattern != "") {
            ul.html(getBreadcrumbFolderItem(node));
            ul.append($("<li />").text(resources.searchBreadcrumb));
        } else {
            ul.html($("<li />").text(node.get_text()).attr("title", node.get_text()));
        }

        while (node.get_value() != rootFolderId) {
            node = node.get_parent();
            ul.prepend(getBreadcrumbFolderItem(node));
        }
    }

    function selectListViewItem(index) {
        var listViewItem = $("#dnnModuleDigitalAssetsListViewItem_" + index, "#" + controls.scopeWrapperId);

        listViewItem.addClass("selected");
        listViewItem.find("input[type=checkbox].dnnModuleDigitalAssetsListViewItemCheckBox").attr("checked", true);
        listViewItem.find(".dnnCheckbox").addClass("dnnCheckbox-checked");
    }

    function deselectListViewItem(index) {
        var listViewItem = $("#dnnModuleDigitalAssetsListViewItem_" + index, "#" + controls.scopeWrapperId);

        listViewItem.removeClass("selected");
        listViewItem.find("input[type=checkbox].dnnModuleDigitalAssetsListViewItemCheckBox").removeAttr("checked");
        listViewItem.find(".dnnCheckbox").removeClass("dnnCheckbox-checked");
    }

    var updateSelectionToolBarTimeout;

    function gridOnRowSelected(sender, args) {

        var index = args.get_itemIndexHierarchical();
        
        var selectCell = $(args.get_item().get_cell("Select"));
        selectCell.find("input[type='checkbox']").attr("checked", true);
        selectCell.find(".dnnCheckbox").addClass("dnnCheckbox-checked");

        // Select corresponding listview item        
        selectListViewItem(index);

        clearTimeout(updateSelectionToolBarTimeout);
        updateSelectionToolBarTimeout = setTimeout(function () {
            var totalItems = grid.get_dataItems().length;
            var totalSelectedItems = grid.get_selectedItems().length;

            if (totalItems == totalSelectedItems) {
                $("#dnnModuleDigitalAssetsListViewToolbar input[type=checkbox]", "#" + controls.scopeWrapperId).attr('checked', true);
                $('#dnnModuleDigitalAssetsListViewToolbar>span.dnnModuleDigitalAssetsListViewToolbarTitle', "#" + controls.scopeWrapperId).text(resources.unselectAll);
            }
            updateSelectionToolBar();
        }, 2);
    }

    function gridOnRowDeselected(sender, args) {

        var index = args.get_itemIndexHierarchical();

        //cancel edition
        var gridItem = sender.get_masterTableView().get_dataItems()[index];
        //If some use case mark the row as continueEditing, don't cancel
        if (gridItem && !$(gridItem.get_element()).hasClass("continueEditing")) {
            cancelRenameInGrid(gridItem.get_id());
        }

        var selectCell = $(args.get_item().get_cell("Select"));
        selectCell.find("input[type='checkbox']").removeAttr("checked");
        selectCell.find(".dnnCheckbox").removeClass("dnnCheckbox-checked");

        // deselect corresponding listview item
        deselectListViewItem(index);

        clearTimeout(updateSelectionToolBarTimeout);
        updateSelectionToolBarTimeout = setTimeout(function () {
            $("#dnnModuleDigitalAssetsListViewToolbar input[type=checkbox]", "#" + controls.scopeWrapperId).attr('checked', false);
            $('#dnnModuleDigitalAssetsListViewToolbar>span.dnnModuleDigitalAssetsListViewToolbarTitle', "#" + controls.scopeWrapperId).text(resources.selectAll);
            updateSelectionToolBar();
        }, 2);
    }

    function createNewFolder(node, sender) {
        $("#dnnModuleDigitalAssetsCreateFolderMessage").hide();

        initializeCreateNewFolderForm(node);
        var $modal = $("#dnnModuleDigitalAssetsCreateFolderModal");
        $modal.dialog({
            modal: true,
            autoOpen: true,
            dialogClass: "dnnFormPopup",
            title: resources.createNewFolderTitleText,
            resizable: false,
            width: 500,
            height: 400,
            buttons: [
                { id: "save_button", text: resources.saveText, click: function () { saveNewFolder(node, sender); }, "class": "dnnPrimaryAction" },
                { id: "cancel_button", text: resources.cancelText, click: function () { $(this).dialog("close"); }, "class": "dnnSecondaryAction" }
            ]
        });        
        $modal.off('keyup').on('keyup', function (e) {
            if (e.keyCode == $.ui.keyCode.ENTER) {
                $modal.parent().find("button.dnnPrimaryAction").trigger("click");
            }
        }).off('keypress').on('keypress', function (e) {    // Required for IE
            if (e.keyCode == $.ui.keyCode.ENTER) {
                e.preventDefault();    
            }
        });
    }

    function initializeCreateNewFolderForm(node) {

        $("#dnnModuleDigitalAssetsCreateFolderModalParent").text(node.get_text()).attr("title", node.get_text());

        var comboFolderType = $find(controls.comboBoxFolderTypeId);
        var spanFolderTypeNoEditable = $("#dnnModuleDigitalAssetsFolderTypeNoEditableLabel");
        var txtFolderName = $("#" + controls.txtFolderNameId);
        var txtMappedName = $("#" + controls.txtMappedPathId);

        txtFolderName.val('');
        txtMappedName.val('');

        resetClientValidations("CreateFolder");

        var folderProviderValue = node.get_value() == rootFolderId && settings.defaultFolderTypeId != '' ? settings.defaultFolderTypeId : node.get_category();

        var item = comboFolderType.findItemByValue(folderProviderValue);
        if (item != null) {
            item.select();
            spanFolderTypeNoEditable.text(item.get_text());
        }

        var defaultFolderProviders = resources.defaultFolderProviderValues == "" ? [] : resources.defaultFolderProviderValues.split(',');
        if ($.inArray(folderProviderValue.toString(), defaultFolderProviders) == -1) //The Folder Provider is not a Default Folder Provider
        {
            comboFolderType.set_visible(false);
            $("#" + controls.comboBoxFolderTypeId).addClass('dnnModuleDigitalAssetsHideFolderTypeDropDowList');
            $("#" + controls.comboBoxFolderTypeId + ">table").hide();
            spanFolderTypeNoEditable.css('display', 'inline-block');
            $("#dnnModuleDigitalAssetsCreateFolderMappedPathFieldRow").hide();
        }
        else {
            comboFolderType.set_visible(true);
            $("#" + controls.comboBoxFolderTypeId).removeClass('dnnModuleDigitalAssetsHideFolderTypeDropDowList');
            $("#" + controls.comboBoxFolderTypeId + ">table").show();
            spanFolderTypeNoEditable.css('display', 'none');
        }
    }

    function resetClientValidations(validationGroup) {
        if (typeof (Page_Validators) != "undefined") {
            for (var i = 0; i < Page_Validators.length; i++) {
                var validator = Page_Validators[i];
                if (validator.validationGroup == validationGroup) {
                    validator.isvalid = true;
                    ValidatorUpdateDisplay(validator);
                }
            }
        }
    }

    function folderTypeComboBoxOnSelectedIndexChanged(sender, eventArgs) {
        var item = eventArgs.get_item();
        var supportsMappedPath = item.get_attributes().getAttribute("SupportsMappedPaths");
        if (!supportsMappedPath) {
            return;
        }

        var txtMappedName = $("#" + controls.txtMappedPathId);
        if (supportsMappedPath != "true") { //If not supports Mapped Paths, then hide Mapped Path field
            $("#dnnModuleDigitalAssetsCreateFolderMappedPathFieldRow").hide();
            txtMappedName.prop('disabled', true);
        } else {
            $("#dnnModuleDigitalAssetsCreateFolderMappedPathFieldRow").show();
            txtMappedName.prop('disabled', false);
            resetClientValidations("CreateFolder");
        }
    }


    function saveNewFolder(node, sender) {
        
        var saveButton = $('#save_button');
        saveButton.button("option", "disabled", true);

        var comboFolderType = $find(controls.comboBoxFolderTypeId);
        var txtFolderName = $("#" + controls.txtFolderNameId);
        var txtMappedName = $("#" + controls.txtMappedPathId);
        var message = $("#dnnModuleDigitalAssetsCreateFolderMessage");

        txtFolderName.val(cleanItemName(txtFolderName.val(), true));

        Page_ClientValidate("CreateFolder");
        if (Page_IsValid) {

            var loadingPanel = $(".dnnModuleDigitalAssetsCreateFolderLoading");
            loadingPanel.show();
            $.ajax({
                url: getContentServiceUrl() + "CreateNewFolder",
                data: {
                    "FolderName": cleanItemName(txtFolderName.val(), true),
                    "ParentFolderId": node.get_value(),
                    "FolderMappingId": comboFolderType.get_value(),
                    "MappedName": cleanItemName(txtMappedName.val(), true)
                },
                async: false,
                type: "POST",
                beforeSend: servicesFramework.setModuleHeaders
            }).done(function (data) {                
                onFolderCreated(data, node.get_value(), sender);
            }).fail(function (xhr, status, error) {
                if (!isXhrHandled(xhr)) {
                    message.html(getExceptionMessage(xhr)).show();
                }
                txtFolderName.select();
            }).always(function () {
                loadingPanel.hide();
            });
        }

        saveButton.button("option", "disabled", false);
    }

    function onFolderCreated(folder, parentFolderId, sender) {
        var parentNode = treeView.findNodeByValue(parentFolderId);
        if (parentNode) {
            treeView.trackChanges();

            if (!parentNode.get_expanded() && sender == "treeview") {
                parentNode.set_expandMode(Telerik.Web.UI.TreeNodeExpandMode.WebService);
                internalOnNodeExpanding(parentNode, true);
                parentNode.set_expanded(true);
            } else {
                var node = createNewNode(folder);
                parentNode.get_nodes().add(node);
                initDroppableNode(node);
            }

            treeView.commitChanges();
            treeViewRefreshScrollbars();
        }

        if (parentFolderId == currentFolderId) {
            loadFolderFirstPage(currentFolderId);
        }
        
        $("#dnnModuleDigitalAssetsCreateFolderModal").dialog('close');
    }

    function isXhrHandled(xhr) {
        if (xhr.status == 401) {
            location.reload();
            return true;
        }
        //If the XHR is handled into this function return true, else return false
        return false;
    }

    function getExceptionMessage(xhr) {

        if (!xhr.responseText) return null;

        try {
            var data = eval("(" + xhr.responseText + ")");

            if (data.ExceptionMessage) {
                return data.ExceptionMessage;
            }
            if (data.Message) {
                return data.Message;
            }
            return null;
        } catch (ex) {
            return xhr.statusText;
        }
    }

    function createFolder() {
        createNewFolder(getCurrentNode());
    }

    function getCurrentFolderPath() {
        var node = getCurrentNode();
        var folderPath = "";

        while (node.get_value() != rootFolderId) {
            folderPath = node.get_text() + '/' + folderPath;
            node = node.get_parent();
        }
        
        return settings.rootFolderPath + folderPath;
    }

    function uploadFiles() {
        fileUpload.uploadFiles();
    }

    function deleteSelectedItems() {
        deleteItems(convertToItemsFromGridItems(grid.get_selectedItems()), currentFolderId);
    }

    function convertToItemsFromGridItems(gridItems) {
        var items = [];
        for (var i = 0; i < gridItems.length; i++) {
            var item = gridItems[i].get_dataItem();
            if (gridItems[i].get_visible() && item) {
                items.push({
                    ItemId: item.ItemID,
                    IsFolder: item.IsFolder,
                    ParentFolderId: item.ParentFolderID
                });
            }
        }

        return items;
    }

    function selectionText(items) {
        var numberOfFiles = 0;
        var numberOfFolders = 0;

        for (var i = 0; i < items.length; i++) {
            if (items[i].IsFolder) {
                numberOfFolders++;
            } else {
                numberOfFiles++;
            }
        }

        var filesText = numberOfFiles == 0 ? '' : numberOfFiles == 1 ? resources.oneFileText : resources.nFilesText.replace('[NUM]', numberOfFiles);
        var foldersText = numberOfFolders == 0 ? '' : numberOfFolders == 1 ? resources.oneFolderText : resources.nFoldersText.replace('[NUM]', numberOfFolders);

        var hasFoldersAndFiles = (numberOfFolders > 0) && (numberOfFiles > 0);
        if (hasFoldersAndFiles) {
            return resources.andText.replace('[FILES]', filesText).replace('[FOLDERS]', foldersText);
        } else {
            return filesText + foldersText;
        }
    }

    function deleteItems(items, parentFolderId) {
        var folderAndFileText = selectionText(items);
        var dialogTitle = resources.deleteTitle.replace('[ITEMS]', folderAndFileText);
        var dialogText = resources.deleteConfirmText.replace('[ITEMS]', folderAndFileText);

        $("<div class='dnnDialog'></div>").html(dialogText).dialog({
            modal: true,
            autoOpen: true,
            dialogClass: "dnnFormPopup",
            width: 400,
            height: 190,
            resizable: false,
            title: dialogTitle,
            buttons:
            [
                {
                    id: "delete_button", text: resources.deleteText, "class": "dnnPrimaryAction", click: function () {
                        $(this).dialog("close");
                        enableLoadingPanel(true);
                        
                        $.ajax({
                            url: getContentServiceUrl() + "DeleteItems",
                            data: {
                                Items: items
                            },
                            type: "POST",
                            beforeSend: servicesFramework.setModuleHeaders
                        }).done(function (data) {
                            onItemsDeleted(items, data, parentFolderId);
                        }).fail(function (xhr) {
                            handledXhrError(xhr, resources.deleteItemsErrorTitle);
                        }).always(function () {
                            enableLoadingPanel(false);                            
                        });
                    }
                },
                { id: "cancel_button", text: resources.cancelText, click: function () { $(this).dialog("close"); }, "class": "dnnSecondaryAction" }
            ]
        });
    }
    
    function onItemsDeleted(items, itemsNotDeleted, parentFolderId) {
        
        // remove nodes from the TreeView
        for (var i = 0; i < items.length; i++) {
            var item = items[i];
            
            if (item.IsFolder) {
                var hasBeenDeleted = true;
                for (var j = 0; j < itemsNotDeleted.length; j++) {
                    if(item.ItemId == itemsNotDeleted[j].ItemId) {
                        hasBeenDeleted = false;
                        break;
                    }
                }
                if (hasBeenDeleted) {
                    var deletedNode = treeView.findNodeByValue(item.ItemId);
                    if (deletedNode) {
                        treeView.trackChanges();

                        var parentNode = deletedNode.get_parent();

                        if (deletedNode.get_value() == currentFolderId) {
                            currentFolderId = parentNode.get_value();
                            parentNode.select();
                        }

                        parentNode.get_nodes().remove(deletedNode);
                        treeView.commitChanges();
                        treeViewRefreshScrollbars();
                    }
                }
            }
        }

        // Check if the system cannot be able to delete some items
        if (itemsNotDeleted.length > 0) {
            alertNotDeletedItems(itemsNotDeleted);
        }

        if (parentFolderId == currentFolderId) {
            loadFolderFirstPage(currentFolderId);
        }
    }

    function alertNotDeletedItems(items) {
        var $table = $("#dnnModuleDigitalAssetsAlertItemsScroll table");
        $table.empty();
        for (var i = 0; i < items.length; i++) {
            $table.append("<tr><td style='width:20px;'><img src='" + items[i].IconUrl + "' alt=''/></td><td>" + items[i].DisplayPath + "</td></tr>");
        }

        $("#dnnModuleDigitalAssetsAlertItems").dialog({
            modal: true,
            autoOpen: true,
            dialogClass: "dnnFormPopup",
            width: 600,
            height: 330,
            resizable: false,
            title: resources.noItemsDeletedTitle.replace("[ITEMS]", selectionText(items)),
            buttons:
            [
                 { id: "close_button", text: resources.closeText, click: function () { $(this).dialog("close"); }, "class": "dnnSecondaryAction" }
            ]
        });

        $("#dnnModuleDigitalAssetsAlertItemsSubtext").html(resources.noItemsDeletedText);
        $("#dnnModuleDigitalAssetsAlertItemsScroll").jScrollPane();
    }

    function moveSelectedItems() {
        moveDialog(convertToItemsFromGridItems(grid.get_selectedItems()));
    }

    function moveDialog(selectedItems) {
        var selText = selectionText(selectedItems);

        resetDestinationTreeView();

        var width = 450;
        var height = 400;

        $("#dnnModuleDigitalAssetsSelectDestinationFolderModal").dialog({
            modal: true,
            autoOpen: true,
            dialogClass: "dnnFormPopup",
            title: resources.moveTitle.replace('[SELECTION]', selText),
            resizable: true,
            width: width,
            height: height,
            minWidth: width,
            minHeight: height,
            maxWidth: width * 1.5,
            maxHeight: height * 1.5,
            buttons: [
                {
                    id: "destination_button",
                    text: resources.moveText,
                    click: function () {
                        $(this).dialog("close");
                        var destinationFolderId = destinationTreeView.get_selectedNode().get_value();
                        moveItems(selectedItems, destinationFolderId);
                    },
                    "class": "dnnPrimaryAction"
                },
                {
                    id: "cancel_button",
                    text: resources.cancelText,
                    click: function () { $(this).dialog("close"); },
                    "class": "dnnSecondaryAction"
                }
            ],
            resize: function () {
                $("#dnnModuleDigitalAssetsDestinationFolderScroll").jScrollPane();
            }
        });

        $("#dnnModuleDigitalAssetsDestinationFolderScroll").jScrollPane();
        destinationMove = true;
        itemsSelectedToMoveOrCopy = selectedItems;
        checkDestinationButtonState(selectedItems, destinationTreeView.get_selectedNode());
    }

    function moveItems(items, destinationFolderId) {
        var ajaxs = [];
        for (var i = 0; i < items.length; i++) {
            ajaxs.push(moveItem(items[i].ItemId, items[i].IsFolder, destinationFolderId, false));
        }

        $.when.apply(null, ajaxs).always(function () {
            loadFolderFirstPage(currentFolderId);
        });
    }

    var moveItem = function (itemId, isFolder, destinationFolderId, overwrite) {
        return $.ajax({
            type: 'POST',
            url: getContentServiceUrl() + (isFolder ? 'MoveFolder' : 'MoveFile'),
            data: {
                itemId: itemId,
                destinationFolderId: destinationFolderId,
                overwrite: overwrite
            },
            beforeSend: servicesFramework.setModuleHeaders
        }).done(function (data) {
            if (data.AlreadyExists) {
                alertDuplicateItems(itemId, data.ItemName, isFolder, destinationFolderId, moveItem);
            } else {
                removeAlert(itemId);

                if (isFolder) {
                    moveNode(itemId, destinationFolderId);
                }
            }
        }).fail(function (xhr) {
            handledXhrError(xhr, resources.moveError);
        });
    };

    function moveNode(nodeId, destinationNodeId) {
        var sourceNode = treeView.findNodeByValue(nodeId);
        var destinationNode = treeView.findNodeByValue(destinationNodeId);
        
        treeView.trackChanges();
        treeView.unselectAllNodes();
        
        var sourceParentNode = null;
        if (sourceNode) {
            sourceParentNode = sourceNode.get_parent();
            sourceParentNode.get_nodes().remove(sourceNode);
        }

        if (destinationNode && destinationNode.get_expandMode() != Telerik.Web.UI.TreeNodeExpandMode.WebService) {
            if (!sourceNode) {
                // create node taking the info from the Grid
                var item = getGridDataItemById(nodeId);
                if (item) {
                    sourceNode = createNewNode({
                        FolderName: item.ItemName,
                        FolderID: item.ItemID,
                        FolderMappingID: item.FolderMappingID,
                        Permissions: item.Permissions,
                        IconUrl: item.IconUrl,
                        HasChildren: item.HasChildren
                    });
                }
            }

            if (sourceNode) {
                destinationNode.get_nodes().add(sourceNode);
                initDroppableNode(sourceNode);
            }
        } else if (sourceNode && sourceNode.get_value() == currentFolderId) {
            currentFolderId = sourceParentNode.get_value();
        }
        
        var selectedNode = treeView.findNodeByValue(currentFolderId);
        if(selectedNode) {
            selectedNode.select();
            if (selectedNode.get_parent() != null && !selectedNode.get_parent().get_expanded()) {
                selectedNode.get_parent().expand();
            }
        }
        treeView.commitChanges();
        treeViewRefreshScrollbars();
    }

    function resetDestinationTreeView() {

        var rootNode = destinationTreeView.findNodeByValue(rootFolderId);
        var mainRootNode = treeView.findNodeByValue(rootFolderId);
        
        rootNode.get_nodes().clear();
        copyNodeSettings(rootNode, mainRootNode);
        cloneNodeRecursive(mainRootNode, rootNode);
        var selectedNode = destinationTreeView.findNodeByValue(currentFolderId);
        if (selectedNode) {
            selectedNode.select();
        }
    }

    function copySelectedItems() {
        var selectedItems = convertToItemsFromGridItems(grid.get_selectedItems());
        var selText = selectionText(selectedItems);

        resetDestinationTreeView();

        var width = 450;
        var height = 400;

        $("#dnnModuleDigitalAssetsSelectDestinationFolderModal").dialog({
            modal: true,
            autoOpen: true,
            dialogClass: "dnnFormPopup",
            title: resources.copyFilesTitle.replace('[SELECTION]', selText),
            resizable: true,
            width: width,
            height: height,
            minWidth: width,
            minHeight: height,
            maxWidth: width * 1.5,
            maxHeight: height * 1.5,
            buttons: [
                {
                    id: "destination_button", text: resources.copyFilesText,
                    click: function () {
                        var destinationFolderId = destinationTreeView.get_selectedNode().get_value();
                        copyItems(selectedItems, destinationFolderId);
                        $(this).dialog("close");
                    }, "class": "dnnPrimaryAction"
                },
                { id: "cancel_button", text: resources.cancelText, click: function () { $(this).dialog("close"); }, "class": "dnnSecondaryAction" }
            ],
            resize: function () {
                $("#dnnModuleDigitalAssetsDestinationFolderScroll").jScrollPane();
            }
        });

        $("#dnnModuleDigitalAssetsDestinationFolderScroll").jScrollPane();
        destinationMove = false;
        itemsSelectedToMoveOrCopy = selectedItems;
        checkDestinationButtonState(selectedItems, destinationTreeView.get_selectedNode());

        $("#dnnModuleDigitalAssetsAlertItemsSubtext").html(resources.duplicateCopySubtext);
        $("#dnnModuleDigitalAssetsAlertItemsScroll table").empty();
    }

    function copyItems(items, destinationFolderId) {
        var ajaxs = [];
        for (var i = 0; i < items.length; i++) {
            if (!items[i].IsFolder) {
                ajaxs.push(copyItem(items[i].ItemId, false, destinationFolderId, false));
            }
        }

        enableLoadingPanel(true);
        $.when.apply($, ajaxs).always(function () {
            if (destinationFolderId == currentFolderId) {
                loadFolderFirstPage(currentFolderId);
            }
            enableLoadingPanel(false);
        });
    }

    var copyItem = function (itemId, isFolder, destinationFolderId, overwrite) {
        return $.ajax({
            type: 'POST',
            url: getContentServiceUrl() + 'CopyFile',
            data: {
                itemId: itemId,
                destinationFolderId: destinationFolderId,
                overwrite: overwrite
            },
            beforeSend: servicesFramework.setModuleHeaders
        }).done(function (data) {
            if (data.AlreadyExists) {
                alertDuplicateItems(itemId, data.ItemName, isFolder, destinationFolderId, copyItem);
            } else {
                removeAlert(itemId);
            }
        }).fail(function (xhr) {
            handledXhrError(xhr, resources.copyError);
        });
    };

    function alertDuplicateItems(itemId, itemName, isFolder, destinationFolderId, replaceFunction) {
        
        $("#dnnModuleDigitalAssetsAlertItemsSubtext").html(resources.duplicateMoveSubtext);
        var $table = $("#dnnModuleDigitalAssetsAlertItemsScroll table");

        $("#dnnModuleDigitalAssetsAlertItems").dialog({
            modal: true,
            autoOpen: true,
            dialogClass: "dnnFormPopup",
            width: 600,
            height: 330,
            resizable: false,
            title: resources.duplicateFilesExistText,
            buttons:
            [
                {
                    id: "replace_button",
                    text: resources.replaceAllText,
                    click: function () {
                        var ajaxs = [];
                        $table.find("tr").each(function () {
                            var fId = $(this).attr("id");
                            var dDestinationFolderId = $(this).attr("data-destinationFolderId");
                            var dIsFolder = $(this).attr("data-isFolder") == "true";
                            ajaxs.push(replaceFunction(fId, dIsFolder, dDestinationFolderId, true));
                        });
                        $(this).dialog("close");
                        $.when.apply($, ajaxs).always(function () {
                            loadFolderFirstPage(currentFolderId);
                        });
                    }, "class": "dnnSecondaryAction"
                },
                {
                    id: "keep_button",
                    text: resources.keepAllText,
                    click: function () {
                        $(this).dialog("close");
                    }, "class": "dnnSecondaryAction"
                }
            ],
            close: function () {
                $table.empty();
            }
        });

        $table.append("<tr id='" + itemId + "' data-isFolder='" + isFolder + "' data-destinationFolderId='" + destinationFolderId + "'>" +
            "<td style='width: 100%;'>" + itemName + "</td>" +
            "<td><a href='#' class='replace'>" + resources.replaceText + "</a>" +
                "<a href='#' class='keep'>" + resources.keepText + "</a></td></tr>");

        $table.find("tr#" + itemId + " a.replace").click(function (e) {
            removeAlert(itemId);
            $.when(replaceFunction(itemId, isFolder, destinationFolderId, true)).always(function () {
                loadFolderFirstPage(currentFolderId);
            });
            e.preventDefault();
        });

        $table.find("tr#" + itemId + " a.keep").click(function (e) {
            removeAlert(itemId);
            e.preventDefault();
        });

        $("#dnnModuleDigitalAssetsAlertItemsScroll").jScrollPane();
    }

    function removeAlert(itemId) {
        var $table = $("#dnnModuleDigitalAssetsAlertItemsScroll table");
        $table.find("tr#" + itemId).remove();
        $("#dnnModuleDigitalAssetsAlertItemsScroll").jScrollPane();
        if ($table.find("tr").length == 0 && $("#dnnModuleDigitalAssetsAlertItems").data("ui-dialog")) {
            $("#dnnModuleDigitalAssetsAlertItems").dialog("close");
        }
    }

    function getUrl() {
        var items = convertToItemsFromGridItems(grid.get_selectedItems());
        var itemId = items[0].ItemId;
        if (!items[0].IsFolder) {            
            getUrlFromFileId(itemId);
        }
    }

    function showProperties() {
        var items = convertToItemsFromGridItems(grid.get_selectedItems());
        showPropertiesDialog(items[0].ItemId, items[0].IsFolder);
    }

    function getFullUrl(relativePath) {
        return location.protocol + '//' + location.hostname + (location.port ? ':' + location.port : '') + relativePath;
    }

    function getUrlFromFileId(fileId) {
        $.ajax({
            type: 'POST',
            url: getContentServiceUrl() + 'GetUrl',
            data: {
                fileId: fileId
            },
            beforeSend: servicesFramework.setModuleHeaders
        }).done(function(data) {
            var url;
            if (data.indexOf("http://") == 0 || data.indexOf("https://") == 0) {
                url = data;
            } else {
                url = getFullUrl(data);
            }
            openGetUrlModal(url, resources.getFileUrlLabel);
        }).fail(function(xhr) {
            handledXhrError(xhr, resources.getUrlErrorTitle);
        });
    }

    function openGetUrlModal(url, label) {
        $('#dnnModuleDigitalAssetsGetUrlModal input').val(url).select();
        $('#dnnModuleDigitalAssetsGetUrlModal span').text(label);
        $('#dnnModuleDigitalAssetsGetUrlModal').dialog({
            modal: true,
            autoOpen: true,
            dialogClass: "dnnFormPopup",
            width: 500,
            height: 250,
            resizable: false,
            title: resources.getUrlTitle,
            buttons:
                [
                    {
                        id: "close_button",
                        text: resources.closeText,
                        click: function () {
                            $(this).dialog("close");
                        },
                        "class": "dnnSecondaryAction"
                    }
                ]
        });
    }

    function getTimeStamp() {
        var timestamp = new Date();
        timestamp = timestamp.getTime();
        return "timestamp=" + timestamp;
    }

    function setTimeStamp(url) {
        if (url.indexOf("?") == -1) {
            return url + "?" + getTimeStamp();
        }
        return url + "&" + getTimeStamp();
    }

    var $idown;
    function downloadUrl(url) {
        url += "&forceDownload=true";                
        if ($idown) {
            $idown.attr('src', url);
        } else {
            $idown = $('<iframe>', { id: 'idown', src: url }).hide().appendTo('body');
        }
    }

    function download() {
        var items = convertToItemsFromGridItems(grid.get_selectedItems());

        controller.download(items,
        {
            downloadUrl: downloadUrl,
            showAlertDialog: showAlertDialog,
            enableLoadingPanel: enableLoadingPanel,
            getExceptionMessage: getExceptionMessage
        });
    }

    function contextMenuOnItemClicked(sender, args) {

        switch (args.get_item().get_value()) {
            case "Rename":
                rename();
                break;

            case "Download":
                download();
                break;

            case "Delete":
                deleteSelectedItems();
                break;

            case "Copy":
                copySelectedItems();
                break;

            case "Move":
                moveSelectedItems();
                break;

            case "GetUrl":
                getUrl();
                break;

            case "UnzipFile":
                unzipFile();
                break;

            case "Properties":
                showPropertiesDialog(grid.get_selectedItems()[0].get_dataItem().ItemID, grid.get_selectedItems()[0].get_dataItem().IsFolder);
                break;
                
            default:
                controller.executeCommandOnSelectedItems(args.get_item().get_value(), grid.get_selectedItems());
                break;
        }
    }
    
    function executeCommandOnSelectedItems(commandName) {
        controller.executeCommandOnSelectedItems(commandName, grid.get_selectedItems());
    }

    function setView(view) {
        if (view == listViewMode) {
            currentView = listViewMode;
            activeListView(true);
            activeGridView(false);
        } else if (view == gridViewMode) {
            currentView = gridViewMode;
            activeListView(false);
            activeGridView(true);
        }        
        var state = controller.getCurrentState(grid, currentView);        
        controller.updateModuleState(state);
        treeViewRefreshScrollbars();
    }

    function listViewInitialize() {
        if (currentView == listViewMode) {
            moreItemsHint();
            $(".dnnModuleDigitalAssetsThumbnail > img", "#" + controls.scopeWrapperId).each(function () {
                $(this).attr('src', $(this).attr('data-src'));
            });
            
            $('#dnnModuleDigitalAssetsListViewContainer input[type="checkbox"]', "#" + controls.scopeWrapperId).dnnCheckbox();
        }
    }

    function activeListView(enabled) {
        var button = $("#DigitalAssetsListViewBtnId span", "#" + controls.scopeWrapperId);
        var control = $("#dnnModuleDigitalAssetsListViewContainer", "#" + controls.scopeWrapperId);

        activeView(enabled, control, button, settings.listViewActiveImageUrl, settings.listViewInactiveImageUrl);
        listViewInitialize();
    }

    function activeGridView(enabled) {
        var button = $("#DigitalAssetsGridViewBtnId span", "#" + controls.scopeWrapperId);
        var control = $("#" + controls.gridId + " > table > thead, #" + controls.gridId + " > table > tbody", "#" + controls.scopeWrapperId);

        activeView(enabled, control, button, settings.gridViewActiveImageUrl, settings.gridViewInactiveImageUrl);
    }

    function activeView(enabled, control, button, activeImageUrl, inactiveImageUrl) {
        if (enabled) {
            button.css("background-image", "url(" + activeImageUrl + ")");
            control.show();
        } else {
            button.css("background-image", "url(" + inactiveImageUrl + ")");
            control.hide();
        }
    }

    function listviewOnContextMenu(item, event) {

        var itemIndex = $(item).attr("data-index");

        if ((!event.relatedTarget) || (!$telerik.isDescendantOrSelf(contextMenu.get_element(), event.relatedTarget))) {

            setupContextMenu(itemIndex, event);
        }

        $telerik.cancelRawEvent(event);
    }

    function listviewOnClick(item, event) {
        var index = $(item).attr("data-index");
        var dataItem = grid.get_dataItems()[index];

        if (dataItem) {
            var target = dataItem.get_element();
            if (event.target.tagName == "INPUT" && event.target.type == "checkbox") {                
                toggleGridItemSelection(dataItem);
            } else if (event.ctrlKey) {
                triggerMouseClick(target, true, false, false);
            } else if (event.shiftKey) {
                triggerMouseClick(target, false, false, true);
            } else {
                triggerMouseClick(target, false, false, false);
            }
        }
    }

    function listviewSelectAllOnClick(e) {
        gridSelectUnselectAll.trigger("click");
    }

    function toggleColumn(columnName, visible) {        
        var index = grid.getColumnByUniqueName(columnName).get_element().cellIndex;
        if (visible) {
            grid.showColumn(index);
        } else {
            grid.hideColumn(index);
        }
    }
    
    function prepareForFilteredContent(hideSync) {
        grid.clearSelectedItems();
        toggleColumn('LastModifiedOnDate', false);
        toggleColumn('ParentFolder', !settings.isFilteredContent);
        
        $('#dnnModuleDigitalAssetsMainToolbar .folderRequired', "#" + controls.scopeWrapperId).hide();
        if (hideSync === true) {
            $('#dnnModuleDigitalAssetsMainToolbar .DigitalAssetsMenuButton_menu', "#" + controls.scopeWrapperId)
                .find('#Sync, #SyncRecursively').addClass("permission_denied");
        }
    }
    
    function getController() {
        return controller;
    }
    
    function initDragAndDropGridSelection() {
        var items = grid.get_selectedItems();
        for (var i = 0; i < items.length; i++) {
            if (!checkSinglePermission(items[0].get_dataItem().Permissions, "COPY")) {
                return false;
            }
        }
        return true;
    }

    function getPathFromNode(node) {
        if (node.get_value() == rootFolderId) {
            return '';
        }
        
        var text = node.get_text();
        for (var n = node.get_parent() ; n.get_parent().get_parent() != null; n = n.get_parent()) {
            text = n.get_text() + '/' + text;
        }

        return text;
    }

    function canDropOnItem(item, sourceNode) {

        if (sourceNode != null) {

            if (sourceNode.get_value() == item.ItemID) {
                return false;
            }

            if (!sourceNode.get_parent() || sourceNode.get_parent().get_value() == item.ItemID) {
                return false;
            }

            // Check if destination is descendant of source
            if (!currentFolder || currentFolder.FolderPath.indexOf(getPathFromNode(sourceNode)) == 0) {
                return false;
            }
        }

        return item.IsFolder
            && (checkSinglePermission(item.Permissions, "ADD") || checkSinglePermission(item.Permissions, "WRITE"));
    }

    function canDropOnNode(sourceNode, node) {
        if (!nodeHasPermission(node, "ADD")) {
            return false;
        }

        if (sourceNode != null) {
            // Folder cannot be copied or moved to its Parent Folder
            if (node.get_value() == sourceNode.get_parent().get_value()) {
                return false;
            }

            // Folder cannot be copied or moved to the same Folder or nested folders
            if (isSubNode(sourceNode.get_value(), node)) {
                return false;
            }

            return true;
        }

        var selectedItems = grid.get_selectedItems();

        // The selection can't be moved if any item has the same parent folder            
        for (var i = 0; i < selectedItems.length; i++) {
            if (selectedItems[i].get_dataItem().ParentFolderID == node.get_value()) {
                return false;
            }
        }

        if (!checkDestinationPermissions(convertToItemsFromGridItems(selectedItems), node)) {
            return false;
        }

        return true;
    }

    var dndTreeExpandTimeout;

    function onItemHelper(event, ui) {
        var index = $(event.currentTarget).data("index");
        if (!grid.get_dataItems()[index].get_selected()) {
            grid.selectItem(grid.get_dataItems()[index].get_element());
        }

        return getDragDropHelper(selectionText(convertToItemsFromGridItems(grid.get_selectedItems())));
    }

    function getDragDropHelper(text) {
        var dragTip = $("<div class='dnnDragdropTip'></div>");
        dragTip.text(resources.moving.replace("[SELECTION]", text));
        return dragTip;
    }

    function onItemDragOver(event, ui) {
        var element = $(event.target);
        var index = element.data("index");
        var dataItem = grid.get_dataItems()[index];

        if (ui.draggable.is(".rtLI")) {
            var sourceNode = treeView.findNodeByValue(ui.draggable.data("FolderId"));

            if (!canDropOnItem(dataItem.get_dataItem(), sourceNode)) {
                element.removeClass("dropTarget");
            }
        }
        else if (!canDropOnItem(dataItem.get_dataItem())) {
            element.removeClass("dropTarget");
        }
    }

    function onItemDrop(event, ui) {

        var index = $(event.target).data("index");
        var item = grid.get_dataItems()[index].get_dataItem();

        if (ui.draggable.is(".rtLI")) {
            var sourceNode = treeView.findNodeByValue(ui.draggable.data("FolderId"));
            var sourceItems = [{
                ItemId: sourceNode.get_value(),
                IsFolder: true,
                ParentFolderId: sourceNode.get_parent().get_value()
            }];

            if (canDropOnItem(item, sourceNode)) {
                moveItems(sourceItems, item.ItemID);
            }
        } else if (canDropOnItem(item)) {
            moveItems(convertToItemsFromGridItems(grid.get_selectedItems()), item.ItemID);
        }
    }

    function initDragAndDrop() {

        $("#dnnModuleDigitalAssetsListContainer", "#" + controls.scopeWrapperId).droppable(
            {
                addClasses: false,
                greedy: true,
                accept: ".dnnModuledigitalAssetsTreeView .rtLI",
                hoverClass: "dropTarget",
                over: function (event, ui) {
                    var sourceNode = treeView.findNodeByValue(ui.draggable.data("FolderId"));
                    if (!currentFolder || !canDropOnItem(currentFolder, sourceNode)) {
                        $(this).removeClass('dropTarget');
                    }
                },
                drop: function (event, ui) {
                    var sourceNode = treeView.findNodeByValue(ui.draggable.data("FolderId"));
                    var sourceItems = [{
                        ItemId: sourceNode.get_value(),
                        IsFolder: true,
                        ParentFolderId: sourceNode.get_parent().get_value()
                    }];

                    if (currentFolder && canDropOnItem(currentFolder, sourceNode)) {
                        moveItems(sourceItems, currentFolder.ItemID);
                    }
                }
            });
    }

    function unzipFile() {
        var item = convertToItemsFromGridItems(grid.get_selectedItems())[0];

        enableLoadingPanel(true);

        $.ajax({
            url: getContentServiceUrl() + "UnzipFile",
            data: {
                FileID: item.ItemId,
                Overwrite: true
            },
            type: "POST",
            beforeSend: servicesFramework.setModuleHeaders
        }).done(function (data) {
            refreshFolder();
        }).fail(function (xhr) {
            handledXhrError(xhr, resources.unzipFileErrorTitle);
        }).always(function () {
            enableLoadingPanel(false);
        });
    }

    $(function () {

        initDragAndDrop();

        if (settings.isFilteredContent === true) {
            $('#DigitalAssetsCreateFolderBtnId').hide();
            $('#DigitalAssetsUploadFilesBtnId').removeClass("rightButton").addClass("singleButton");
        }

        $("#dnnModuleDigitalAssetsListContainer", "#" + controls.scopeWrapperId).mousedown(function (e) {
            if (e.button == 2 && $(e.target).hasClass('emptySpace')) {
                emptySpaceContextMenu(e);
                e.preventDefault();
            }
        });
    });

    return {
        init: init,
        toggleLeftPane: toggleLeftPane,
        destinationTreeViewOnNodeExpanding: destinationTreeViewOnNodeExpanding,
        destinationTreeViewRefreshScrollbars: destinationTreeViewRefreshScrollbars,
        destinationTreeViewOnNodeClicking: destinationTreeViewOnNodeClicking,
        destinationTreeViewOnLoad: destinationTreeViewOnLoad,
        treeViewOnNodeExpanding: treeViewOnNodeExpanding,
        treeViewOnNodeCollapsing: treeViewOnNodeCollapsing,
        treeViewOnNodeClicking: treeViewOnNodeClicking,
        treeViewOnContextMenuItemClicking: treeViewOnContextMenuItemClicking,
        treeViewOnNodeEditing: treeViewOnNodeEditing,
        treeViewOnContextMenuShowing: treeViewOnContextMenuShowing,
        treeViewOnContextMenuShown: onContextMenuShown,
        treeViewOnLoad: treeViewOnLoad,
        treeViewContextMenuOnLoad: treeViewContextMenuOnLoad,
        treeViewContextMenuOnHiding: treeViewContextMenuOnHiding,
        treeViewRefreshScrollbars: treeViewRefreshScrollbars,
        gridOnGridCreated: gridOnGridCreated,
        gridOnCommand: gridOnCommand,
        gridOnRowContextMenu: gridOnRowContextMenu,
        gridOnRowSelected: gridOnRowSelected,
        gridOnRowDeselected: gridOnRowDeselected,
        gridOnRowDataBound: gridOnRowDataBound,
        gridOnDataBound: gridOnDataBound,
        gridOnColumnHidden: gridOnColumnHidden,
        contextMenuOnItemClicked: contextMenuOnItemClicked,
        contextMenuOnLoad: contextMenuOnLoad,
        contextMenuOnShown: onContextMenuShown,
        listviewOnContextMenu: listviewOnContextMenu,
        listviewOnClick: listviewOnClick,
        listViewOnCreated: listViewOnCreated,
        emptySpaceMenuOnLoad: emptySpaceMenuOnLoad,        
        emptySpaceMenuOnItemClicked: emptySpaceMenuOnItemClicked,
        createFolder: createFolder,
        uploadFiles: uploadFiles,
        refreshFolder: refreshFolder,
        showPropertiesFromAction: showPropertiesFromAction,
        deleteSelectedItems: deleteSelectedItems,
        copySelectedItems: copySelectedItems,
        moveSelectedItems: moveSelectedItems,
        download: download,
        rename: rename,
        getUrl: getUrl,
        setView: setView,
        showProperties: showProperties,
        unzipFile: unzipFile,
        highlightItemName: highlightItemName,
        getController: getController,
        prepareForFilteredContent: prepareForFilteredContent,
        itemsDatabind: itemsDatabind,
        enableLoadingPanel: enableLoadingPanel,
        refresFolderFromMenu: refresFolderFromMenu,
        syncFromMenu: syncFromMenu,
        onOpeningRefreshMenu: onOpeningRefreshMenu,
        showDialog: showDialog,
        closeDialog: closeDialog,
        getExceptionMessage: getExceptionMessage,
        showAlertDialog: showAlertDialog,
        isXhrHandled: isXhrHandled,
        getReducedItemName: getReducedItemName,
        getCurrentFolderId: getCurrentFolderId,
        setCurrentFolder: setCurrentFolder,
        folderTypeComboBoxOnSelectedIndexChanged: folderTypeComboBoxOnSelectedIndexChanged,
        executeCommandOnSelectedItems: executeCommandOnSelectedItems,
        setSearchProvider: setSearchProvider,
        getSearchProvider: getSearchProvider,
        getRootFolderId: getRootFolderId,
        createModuleState: createModuleState,
        openGetUrlModal: openGetUrlModal,
        loadInitialContent: loadInitialContent,
        getFullUrl: getFullUrl
    };
}(jQuery, $find, $telerik, dnnModal);