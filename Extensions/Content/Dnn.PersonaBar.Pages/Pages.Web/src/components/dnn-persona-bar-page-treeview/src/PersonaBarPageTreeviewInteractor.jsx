import React, { Component } from "react";
import { connect } from "react-redux";
import { ScrollBar, GridCell } from "@dnnsoftware/dnn-react-common";
import { PersonaBarPageTreeview } from "./PersonaBarPageTreeview";
import { PersonaBarPageTreeMenu } from "./PersonaBarPageTreeMenu";
import { PersonaBarPageTreeParentExpand } from "./PersonaBarPageTreeParentExpand";
import responseStatus from "../../../constants/responseStatus";
import utils from "../../../utils";
import cloneDeep from "lodash/cloneDeep";
import { PropTypes } from "prop-types";
import Promise from "promise";
import "./styles.less";
import Localization from "localization";


class PersonaBarPageTreeviewInteractor extends Component {

    constructor() {
        super();
        this.state = {
            rootLoaded: false,
            isTreeviewExpanded: false,
            initialCollapse: true,
            debounceAmount: 50,
            dragDebounce: false,
            dragOverDebounce: false,
            setMouseCoordDebounce: false,
            pageX: 0,
            pageY: 0,
            isMouseInTree: false,
            isChildLoaded: false,
            treeViewActivePage: {}
        };
        this.origin = window.origin;
        this.treeContentWidth = 200;

        this.countTreeDepthOpen = 0;
    }
    componentDidMount() {
        this.init();
    }
    UNSAFE_componentWillReceiveProps(newProps) {
        let setTreeViewExpanded = null;
        const {
            activePage,
            NoPermissionSelectionPageId
        } = newProps;
        const pageList = cloneDeep(newProps.pageList);
        this.setState({
            pageList: pageList,
            rootLoaded: true
        });
        if (activePage || NoPermissionSelectionPageId) {
            const tabId = (activePage && activePage.tabId) || NoPermissionSelectionPageId;
            this.props._traverse((item, list) => {
                item.selected = false;

                if (item.id === tabId) {
                    item.selected = true;
                    item.includeInMenu = activePage.includeInMenu;
                    this.setState({
                        pageList: list
                    }, () => {
                        this._countTreeOpenDeepParent(this.state.pageList);
                    });
                }
            });
        }
        else {
            this.props._traverse((item, list) => {
                item.selected = false;
                this.setState({
                    pageList: list
                }, () => {
                    this._countTreeOpenDeepParent(this.state.pageList);
                });
            });
        }

        this.props._traverse((item) => {
            if (item.isOpen) {
                setTreeViewExpanded = true;
            }
        });

        (setTreeViewExpanded) ? this.setState({
            isTreeviewExpanded: true,
            initialCollapse: false
        }) : this.setState({
            isTreeviewExpanded: false
        });
    }

    init() {
        this.setState({
            treeViewActivePage: this.props.activePage
        });
    }

    getPageInfo(id) {
        return new Promise((resolve) => {
            const {
                setActivePage,
                getPage
            } = this.props;

            getPage(id)
                .then((data) => {
                    this.setState({
                        treeViewActivePage: data
                    });
                    return setActivePage(data);
                }).then(() => resolve());
        });
    }

    toggleParentCollapsedState(id) {
        this.setState({
            initialCollapse: false
        });
        let listPageItems = undefined;
        this.props._traverse((item, listItem, updateStore) => {
            (item.id === id) ? item.isOpen = !item.isOpen : null;
            updateStore(listItem);
            listPageItems = listItem;
        });

        this._countTreeOpenDeepParent(listPageItems);
    }

    onSelection({ id }) {
        this.props._traverse((item, listItem, updateStore) => {
            (item.id === id && item.canManagePage) ? item.selected = true : item.selected = false;
            item.selected ? this.props.onSelection(id) : null;
            delete item.showInContextMenu;
            updateStore(listItem);
        });
    }

    onNoPermissionSelection({ id }) {
        this.props._traverse((item, list, updateStore) => {
            (item.id === id) ? item.selected = true : item.selected = false;
            item.selected ? this.props.onNoPermissionSelection(id) : null;
            delete item.showInContextMenu;
            updateStore(list);
        });
    }

    onDuplicatePage(listItem) {
        let updateReduxStore = null;
        let pageList = null;
        this.props._traverse((item, list, updateStore) => {
            delete item.showInContextMenu;
            updateReduxStore = updateStore;
            pageList = list;
        });

        updateReduxStore(pageList);
        this.props.onDuplicatePage(listItem);
    }

    getListItemLI(item) {
        const element = document.getElementById(`list-item-${item.name}-${item.id}`);
        return element;
    }

    getListItemTitle(item) {
        const element = document.getElementById(`list-item-title-${item.name}-${item.id}`);
        return element;
    }

    setMouseCoordinates(e) {
        const capture = () => {
            this.setState({ setMouseCoordDebounce: true, pageX: e.pageX, pageY: e.pageY });
            setTimeout(() => this.setState({ setMouseCoordDebounce: false }), this.state.debounceAmount);
        };
        const nothing = () => { };

        !this.state.dragOverDebounce ? capture() : nothing();
    }


    createClonedElement(e, item) {
        const element = this.getListItemLI(item);
        this.clonedElement = element.cloneNode(true);
        this.clonedElement.id = "cloned";
        this.clonedElement.style.transition = "all";
        this.clonedElement.style.top = `${this.state.pageY}px`;
        this.clonedElement.style.left = `${this.state.pageX}px`;
        this.clonedElement.classList.add("dnn-persona-bar-treeview-dragged");
        document.body.appendChild(this.clonedElement);
    }

    onDragEnter(e) {
        e.preventDefault();

    }

    onDragStart(e, item) {

        const userAgent = window.navigator.userAgent;
        let type = "text/plain";

        if (userAgent.indexOf('Trident')) {
            type = 'Text';
        }


        e.dataTransfer.setData ? e.dataTransfer.setData(type, 'node') : null;

        const left = () => {
            const img = new Image();
            if (e.dataTransfer.setDragImage && !userAgent.indexOf("AppleWebkit")) {
                e.dataTransfer.setDragImage(img, 0, 0);
            }

            this.createClonedElement(e, item);
            const self = this;
            this.props._traverse((li, list, updateStore) => {
                li.selected = false;
                delete li.showInContextMenu;
                if (li.id === item.id) {
                    li.selected = true;
                    li.isOpen = false;
                    self.setState({
                        draggedItem: li,
                        pageList: list,
                        treeViewActivePage: item
                    }, () => updateStore(list));
                }
            });
        };

        const right = () => {
            this.props.showCancelDialog(item.id);
        };

        (!this.props.selectedPageDirty) ? left() : right();
    }

    onDrag(e) {
        const move = () => {
            this.setState({ dragDebounce: true });
            const { pageX, pageY } = this.state;
            const elm = this.clonedElement;

            e = { pageX, pageY };
            elm.style.top = `${e.pageY - 10}px`;
            elm.style.left = `${e.pageX - 10}px`;
            setTimeout(() => this.setState({ dragDebounce: false }), this.state.debounceAmount);
        };

        const nothing = () => { };
        !this.state.dragDebounce ? move() : nothing();
    }

    onDragEnd(e) {
        e.preventDefault();

        let pageList = null;
        let runUpdateStore = null;
        this.removeClone();
        this.props._traverse((item, list, updateStore) => {
            item.onDragOverState = false;
            pageList = list;
            runUpdateStore = updateStore;
        });
        this.setState({
            pageList
        }, () => runUpdateStore(pageList));
    }

    onDragLeave(e, item) {
        let pageList = null;
        this.props._traverse((pageListItem, list, updateStore) => {
            if (pageListItem.id === item.id) {
                pageListItem.onDragOverState = false;
                pageList = list;
                this.setState({
                    pageList: pageList
                }, () => updateStore(pageList));
            }
        });
    }

    onDragOver(e, item) {
        e.preventDefault();
        this.setMouseCoordinates(e);
        let pageList = null;

        const action = () => {
            this.setState({ dragOverDebounce: true });
            this.props._traverse((pageListItem, list, updateStore) => {
                pageListItem.onDragOverState = false;
                if (pageListItem.id === item.id) {
                    pageListItem.onDragOverState = true;
                    pageList = list;
                    this.setState({
                        pageList: pageList,
                        dragOverItem: item
                    }, () => updateStore(pageList));
                }
            });
            setTimeout(() => this.setState({ dragOverDebounce: false }), this.state.debounceAmount);
        };

        const noaction = () => { };
        !this.state.dragOverDebounce ? action() : noaction();
    }

    onDrop(e, item) {
        e.preventDefault();
        e.target.classList.remove("list-item-dragover");
        this.removeClone();

        const left = () => {
            let treeViewActivePage = Object.assign({}, this.state.treeViewActivePage);
            let pageList = null;
            let runUpdateStore = null;
            this.props._traverse((pageListItem, list, updateStore) => {
                pageListItem.onDragOverState = false;
                pageList = list;
                runUpdateStore = updateStore;
            });
            this.setState({
                pageList
            }, () => runUpdateStore(pageList));

            this.getPageInfo(treeViewActivePage.id)
                .then(() => {
                    let treeViewActivePage = Object.assign({}, this.state.treeViewActivePage);
                    treeViewActivePage.oldParentId = treeViewActivePage.parentId;
                    treeViewActivePage.parentId = item.id;
                    return this.props.saveDropState(treeViewActivePage);
                })
                .then(this.getPageInfo.bind(this, treeViewActivePage.id))
                .then(() => this.setState({
                    droppedItem: item
                }));
        };

        const right = () => null;
        (item.id !== this.state.draggedItem.id && item.id !== this.state.draggedItem.parentId) ? left() : right();
    }


    onMovePage({
        e,
        Action,
        PageId,
        ParentId,
        RelatedPageId,
        RelatedPageParentId
    }) {

        e.preventDefault();
        const { onMovePage } = this.props;

        onMovePage({
            Action,
            PageId,
            ParentId,
            RelatedPageId
        })
            .then((response) => {
                this.removeDropZones();
                if (response.Status === responseStatus.ERROR) {
                    utils.notifyError(response.Message, 3000);
                    return 0;
                }
                utils.notify(Localization.get("PageUpdatedMessage"));
                if (RelatedPageParentId === -1 && ParentId !== -1 && utils.getCurrentPageId() === response.Page.id) {
                    window.parent.location = response.Page.url;
                }
                return 1;
            }).then((response) => {
                response === 1 && this.reOrderPage({
                    Action,
                    PageId,
                    ParentId,
                    RelatedPageId
                });
            });
    }

    removeClone() {
        this.clonedElement ? document.body.removeChild(this.clonedElement) : null;
        this.clonedElement = null;
    }

    removeDropZones() {
        return new Promise((resolve) => {
            let pageList = null;
            let runUpdateStore = null;
            this.props._traverse((item, list, updateStore) => {
                item.onDragOverState = false;
                pageList = list;
                runUpdateStore = updateStore;
            });

            this.setState({
                pageList
            }, () => {
                runUpdateStore(pageList);
                resolve();
            });
        });
    }

    reOrderPage({
        Action,
        PageId,
        ParentId,
        RelatedPageId
    }) {
        return new Promise((resolve) => {

            let cachedItem = null;
            let itemIndex = null;
            let pageList = null;
            let newParentId = null;
            let newSiblingIndex = null;
            let runUpdateStore = null;

            const removeFromPageList = () => new Promise((rez) => {
                this.props._traverse((item, list, updateStore) => { // remove item from pagelist and cache
                    runUpdateStore = updateStore;
                    switch (true) {
                        case item.id === RelatedPageId && Action === "before":
                            newParentId = item.parentId;
                            this.props._traverse((child) => {
                                if (child.id === PageId) {
                                    const parentId = child.parentId;
                                    this.props._traverse((parent, list) => {
                                        if (parent.id === parentId) {
                                            parent.childListItems.forEach((elm, index) => {
                                                if (elm.id === child.id) {
                                                    cachedItem = child;
                                                    const arr1 = parent.childListItems.slice(0, index);
                                                    const arr2 = parent.childListItems.slice(index + 1);
                                                    const copy = [...arr1, ...arr2];
                                                    parent.childCount--;
                                                    parent.childListItems = copy;
                                                    pageList = list;
                                                }
                                            });
                                        }
                                    });
                                }

                            });

                            break;

                        case item.id === RelatedPageId:
                            newParentId = item.parentId;
                            return;

                        case ParentId === -1 && item.parentId === -1:
                            list.forEach((child, index) => {
                                if (child.id === PageId) {
                                    cachedItem = child;
                                    itemIndex = index;
                                    const arr1 = list.slice(0, index);
                                    const arr2 = list.slice(index + 1);
                                    const copy = [...arr1, ...arr2];
                                    pageList = copy;
                                }
                            });
                            return;

                        case item.id === ParentId:
                            item.childListItems.forEach((child, index) => {
                                if (child.id === PageId) {
                                    child.selected = true;
                                    cachedItem = child;
                                    itemIndex = index;
                                    const arr1 = item.childListItems.slice(0, itemIndex);
                                    const arr2 = item.childListItems.slice(itemIndex + 1);
                                    item.childCount--;
                                    item.childListItems = [...arr1, ...arr2];
                                    pageList = list;
                                }
                            });
                            return;

                        default:
                            list.forEach((item) => {
                                if (item.id === ParentId) {
                                    item.childListItems.forEach((child, index) => {
                                        if (child.id === PageId) {
                                            cachedItem = child;
                                            itemIndex = index;
                                            item.childCount--;
                                            const arr1 = item.childListItems.slice(0, itemIndex);
                                            const arr2 = item.childListItems.slice(itemIndex + 1);
                                            item.childListItems = [...arr1, ...arr2];
                                            pageList = list;
                                        }
                                    });
                                }
                            });
                    }
                });

                this.setState({
                    pageList: pageList
                }, () => {
                    this.getPageInfo(cachedItem.id).then(() => {
                        cachedItem.url = `${window.origin}/${this.props.activePage.url}`;
                        if (pageList)
                            runUpdateStore(pageList);
                        rez();
                    });
                });
            });


            const updateNewParent = () => new Promise((rez) => {
                this.props._traverse((item, list, updateStore) => {
                    runUpdateStore = updateStore;
                    switch (true) {
                        case item.id === newParentId:
                            item.childListItems.forEach((child, index) => {
                                if (child.id === RelatedPageId) {
                                    newSiblingIndex = index;
                                    item.childCount++;
                                    (Action === "after") ? newSiblingIndex++ : null;

                                    const arr1 = item.childListItems.slice(0, newSiblingIndex);
                                    const arr2 = item.childListItems.slice(newSiblingIndex);
                                    cachedItem.parentId = item.id;
                                    item.childListItems = [...arr1, cachedItem, ...arr2];
                                    pageList = list;
                                }
                            });
                            return;
                        case ParentId === -1 || newParentId === -1:
                            list.forEach((child, index) => {
                                if (child.id === RelatedPageId) {
                                    newSiblingIndex = index;
                                    (Action === "after") ? newSiblingIndex++ : null;

                                    const arr1 = list.slice(0, newSiblingIndex);
                                    const arr2 = list.slice(newSiblingIndex);
                                    cachedItem.parentId = -1;
                                    const listCopy = [...arr1, cachedItem, ...arr2];
                                    pageList = listCopy;
                                }
                            });
                            return;
                        default:
                            list.forEach((child, index) => {
                                if (child.id === RelatedPageId && child.parentId === -1) {
                                    newSiblingIndex = index;
                                    (Action === "after") ? newSiblingIndex++ : null;

                                    const arr1 = list.slice(0, index);
                                    const arr2 = list.slice(index);
                                    cachedItem.parentId = -1;
                                    const listCopy = [...arr1, cachedItem, ...arr2];
                                    pageList = listCopy;
                                }
                            });
                    }
                });
                this.setState({
                    pageList
                }, () => {
                    runUpdateStore(pageList);
                    rez();
                });
            });

            removeFromPageList()
                .then(() => updateNewParent())
                .then(() => resolve());
        });
    }

    _countTreeOpenDeepParent(listItems) {
        let maxCount = 0;
        for (let i = 0; i < listItems.length; i++) {
            let item = listItems[i];
            if (item.isOpen) {
                if (item.childListItems) {
                    let count = this._countTreeDeep(item.childListItems);
                    if (count > maxCount) {
                        maxCount = count;
                    }
                }
            }
        }
        this.countTreeDepthOpen = maxCount + 1;
        return this.countTreeDepthOpen;
    }

    _countTreeDeep(listItems) {
        let count = 0;

        for (let i = 0; i < listItems.length; i++) {
            let item = listItems[i];
            if (item.isOpen) {
                count++;
                if (item.childListItems) {
                    count += this._countTreeDeep(item.childListItems);
                }
                return count;
            }
        }
        return count;
    }

    getChildListItems(id) {
        let promise = this.loadAllChildListItems(id);
        promise.then(this.toggleParentCollapsedState(id));
    }

    loadAllChildListItems(id) {
        return new Promise((resolve) => {
            const getChildListItems = () => {
                this.props.getChildPageList(id)
                    .then((childListItems) => {
                        this.props._traverse((item, listItems, updateStore) => {
                            const getChildListItemsService = () => {
                                item.childListItems = childListItems;
                            };
                            (item.id === id) ? getChildListItemsService() : null;
                            if (item.id === id) {
                                this.setState({
                                    pageList: listItems
                                }, () => {
                                    updateStore(listItems);
                                    resolve();
                                });
                            }
                        });
                    });
            };
            this.props._traverse((item) => (item.childCount > 0 && item.id === id && !item.hasOwnProperty('childListItems')) ? getChildListItems() : resolve());
        });
    }

    loadAllChildList() {
        let promises = [];
        this.props._traverse((item) => {
            if (item.childCount > 0 && !item.hasOwnProperty('childListItems')) {
                promises.push(this.loadAllChildListItems(item.id));
            }
        });

        if (promises.length > 0) {
            Promise.all(promises).then(this.setState({ isChildLoaded: true }));
        }
    }

    toggleExpandAll() {
        const {
            isTreeviewExpanded
        } = this.state;

        this.props._traverse((item, list, updateStore) => {
            if (item.hasOwnProperty("childListItems") && item.childListItems.length > 0) {
                item.isOpen = (isTreeviewExpanded) ? false : true;
                updateStore(list);
                this.setState({
                    isTreeviewExpanded: !this.state.isTreeviewExpanded
                });
            }
        });
    }

    render_treeview() {
        return (
            <span className="dnn-persona-bar-treeview-ul tree" onMouseOver={(e) => this.props.enabled && this.setState({ pageX: e.pageX, pageY: e.pageY })} style={{ paddingBottom: "10px" }}>
                {this.state.rootLoaded ?
                    <PersonaBarPageTreeview
                        draggedItem={this.state.draggedItem}
                        droppedItem={this.state.droppedItem}
                        dragOverItem={this.state.dragOverItem}
                        listItems={this.state.pageList}
                        setEmptyPageMessage={this.props.setEmptyPageMessage}
                        getChildListItems={this.getChildListItems.bind(this)}
                        onSelection={this.props.enabled && this.onSelection.bind(this)}
                        onNoPermissionSelection={this.props.enabled && this.onNoPermissionSelection.bind(this)}
                        onDragEnter={this.props.enabled && this.onDragEnter.bind(this)}
                        onDrag={this.props.enabled && this.onDrag.bind(this)}
                        onDragStart={this.props.enabled && this.onDragStart.bind(this)}
                        onDragOver={this.props.enabled && this.onDragOver.bind(this)}
                        onDragLeave={this.props.enabled && this.onDragLeave.bind(this)}
                        onDragEnd={this.props.enabled && this.onDragEnd.bind(this)}
                        onDrop={this.props.enabled && this.onDrop.bind(this)}
                        onMovePage={this.props.enabled && this.onMovePage.bind(this)}
                        getPageInfo={this.getPageInfo.bind(this)}
                        selectedPageDirty={this.props.selectedPageDirty}
                        Localization={this.props.Localization}
                    />
                    : null}
            </span>
        );
    }

    render_treemenu() {
        return (
            <span className="dnn-persona-bar-treeview-ul" >
                {this.state.rootLoaded ?
                    <PersonaBarPageTreeMenu
                        CallCustomAction={this.props.enabled && this.props.CallCustomAction}
                        onAddPage={this.props.enabled && this.props.onAddPage}
                        onViewPage={this.props.enabled && this.props.onViewPage}
                        onViewEditPage={this.props.enabled && this.props.onViewEditPage}
                        onDuplicatePage={this.props.enabled && this.onDuplicatePage.bind(this)}
                        listItems={this.state.pageList}
                        _traverse={this.props._traverse.bind(this)}
                        pageInContextComponents={this.props.pageInContextComponents}
                    /> : null}

            </span>
        );
    }

    render_tree_parent_expand() {
        return (
            <span
                className="dnn-persona-bar-treeview-ul" >
                {this.state.rootLoaded ? <PersonaBarPageTreeParentExpand listItems={this.state.pageList} getChildListItems={this.getChildListItems.bind(this)} /> : null}
            </span>
        );
    }

    render_collapseExpand() {
        return (
            <div
                onClick={this.props.enabled && this.toggleExpandAll.bind(this)}
                className="collapse-expand" >
                [{this.state.isTreeviewExpanded ? Localization.get("lblCollapseAll").toUpperCase() : Localization.get("lblExpandAll").toUpperCase()}]
            </div>
        );
    }

    setMouseOver(isMouseOver) {
        let hasChildren = this.state.pageList && this.state.pageList.some((page) => page.childCount > 0);

        this.setState({
            isMouseInTree: (isMouseOver && hasChildren)
        });
    }

    calculateTreeContentArea() {
        return this.treeContentWidth + (this.countTreeDepthOpen * 25);
    }

    render() {
        if (!this.state.isChildLoaded) {
            this.loadAllChildList();
        }

        return (
            <div onMouseEnter={() => this.setMouseOver(true)} onMouseLeave={() => this.setMouseOver(false)}>
                <GridCell
                    columnSize={30}
                    className="dnn-persona-bar-treeview"
                    style={{ "zIndex": 1000 }} >

                    {this.render_collapseExpand()}

                    <GridCell columnSize={15} >
                        <div className="dnn-persona-bar-treeview-menu" >
                            {this.render_tree_parent_expand()}
                        </div>
                    </GridCell>

                    <GridCell
                        columnSize={55}
                        style={{ marginLeft: "-2px" }} >
                        <ScrollBar contentStyle={{ width: this.calculateTreeContentArea() + "px" }}>
                            {this.render_treeview()}
                        </ScrollBar>
                    </GridCell>

                    <GridCell columnSize={30} >
                        <div
                            className="dnn-persona-bar-treeview-menu selection-arrows"
                            style={{ float: "right" }} >
                            {this.render_treemenu()}
                        </div>
                    </GridCell>

                </GridCell>
            </div>
        );
    }
}

PersonaBarPageTreeviewInteractor.propTypes = {
    _traverse: PropTypes.func.isRequired,
    clearSelectedPage: PropTypes.func.isRequired,
    pageList: PropTypes.array.isRequired,
    showCancelDialog: PropTypes.func.isRequired,
    setEmptyPageMessage: PropTypes.func.isRequired,
    selectedPageDirty: PropTypes.bool.isRequired,
    activePage: PropTypes.object,
    getPage: PropTypes.func.isRequired,
    onSelection: PropTypes.func.isRequired,
    onMovePage: PropTypes.func.isRequired,
    onAddPage: PropTypes.func.isRequired,
    onViewPage: PropTypes.func.isRequired,
    onViewEditPage: PropTypes.func.isRequired,
    onDuplicatePage: PropTypes.func.isRequired,
    CallCustomAction: PropTypes.func.isRequired,
    setActivePage: PropTypes.func.isRequired,
    saveDropState: PropTypes.func.isRequired,
    getChildPageList: PropTypes.func.isRequired,
    getPageList: PropTypes.func,
    pageInContextComponents: PropTypes.array.isRequired,
    Localization: PropTypes.object.isRequired,
    onNoPermissionSelection: PropTypes.func.isRequired,
    NoPermissionSelectionPageId: PropTypes.number,
    enabled: PropTypes.bool
};

PersonaBarPageTreeviewInteractor.defaultProps = {
    enabled: true
};

PersonaBarPageTreeviewInteractor.contextTypes = {
    scrollArea: PropTypes.object
};


const mapStateToProps = (state) => {
    return ({
        activePage: state.pages.selectedPage
    });
};
// export default PersonaBarPageTreeviewInteractor;
export const connectedPersonaBarPageTreeviewInteractor = connect(mapStateToProps)(PersonaBarPageTreeviewInteractor);