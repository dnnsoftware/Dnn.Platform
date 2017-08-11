import React, { Component } from "react";
import Scrollbars from "react-custom-scrollbars";
import { PersonaBarPageTreeview } from "./PersonaBarPageTreeview";
import { PersonaBarPageTreeMenu} from "./PersonaBarPageTreeMenu";
import { PersonaBarPageTreeParentExpand} from "./PersonaBarPageTreeParentExpand";

import { PropTypes } from "prop-types";
import Promise from "promise";
import GridCell from "dnn-grid-cell";
import "./styles.less";


export class PersonaBarPageTreeviewInteractor extends Component {

    constructor() {
        super();
        this.state = {
            rootLoaded: false,
            isTreeviewExpanded: true,
            initialCollapse: true
        };
        this.origin = window.origin;
    }

    componentDidMount() {
        this.init();

    }

    componentWillReceiveProps(newProps){
        let setTreeViewExpanded = null;
        const {activePage} = newProps;
        const pageList = JSON.parse(JSON.stringify(newProps.pageList));
        this.setState({ pageList: pageList, rootLoaded: true });
       
        if(activePage){
            this.props._traverse((item, list, updateStore) => {
                item.selected=false;
                if(item.id === activePage.tabId){
                    item.selected=true; 
                    this.setState({pageList:list});
                }
            });
        }

        if(!this.state.initialCollapse){
            this.props._traverse((item) => {
                if(item.isOpen){
                    setTreeViewExpanded = true;
                }
            });

            (setTreeViewExpanded) ? this.setState({isTreeviewExpanded:true}) : this.setState({isTreeviewExpanded:false});
        }
    }

    init(){
        this.setState({activePage: this.props.activePage});
    }

    GET(url, setState) {
        return new Promise((resolve, reject) => {
            function reqListener() {
                const data = JSON.parse(this.responseText);
                resolve(data);
            }
            const xhr = new XMLHttpRequest();
            xhr.addEventListener("load", reqListener);
            xhr.open("GET", url);
            xhr.send();
        });
    }


    _traverse(comparator) {
        let listItems = this.state.pageList.concat();
        const cachedChildListItems = [];
        cachedChildListItems.push(listItems);
        const condition = cachedChildListItems.length > 0;

        const loop = () => {
            const childItem = cachedChildListItems.length ? cachedChildListItems.shift() : null;
            const left = () => childItem.forEach(item => {
                comparator(item, listItems);
                Array.isArray(item.childListItems) ? cachedChildListItems.push(item.childListItems) : null;
                condition ? loop() : exit();
            });
            const right = () => null;
            childItem ? left() : right();
        };

        const exit = () => null;

        loop();
        return;
    }


    getPageInfo(id) {
        return new Promise((resolve) => {
            const { setActivePage } = this.props;
            const url = `${window.origin}/API/PersonaBar/${window.dnn.pages.apiController}/GetPageDetails?pageId=${id}`;
            this.GET(url)
                .then((data) => {
                    this.setState({ activePage: data });
                    return setActivePage(data);
                })
                .then(() => resolve());
        });
    }

    toggleParentCollapsedState(id) {
        this.setState({initialCollapse:false});
        let setTreeViewExpanded = null;
        this.props._traverse((item, listItem, updateStore) => {
            (item.id === id) ? item.isOpen = !item.isOpen : null;
            updateStore(listItem);
        });

    }

    onSelection(id) {
        this.props._traverse((item, listItem, updateStore) => {
            (item.id === id && item.canViewPage) ? item.selected = true : item.selected = false;
             item.selected ?  this.props.onSelection(id) : null;
            updateStore(listItem);
        });
    }

    getListItemLI(item) {
        return document.getElementById(`list-item-${item.name}-${item.id}`);
    }

    getListItemTitle(item) {
        return document.getElementById(`list-item-title-${item.name}-${item.id}`);
    }

    _fadeOutTooltips(){
         const tooltips = document.getElementsByClassName("__react_component_tooltip");
         for(let i=0; i<tooltips.length; i++){
            tooltips[i].style.transition="all .5s";
            tooltips[i].style.opacity=0;
         }
    }

     _fadeInTooltips(){
         const tooltips = document.getElementsByClassName("__react_component_tooltip");
         const run = () => {
             for (let i=0; i<tooltips.length; i++){
                tooltips[i].style.opacity=1;
             }
         };

         setTimeout(()=>run(), 1000);
    }



    onDragStart(e, item) {


        this._fadeOutTooltips();

        const left = () => {
            const img = new Image();
            e.dataTransfer.setDragImage(img, 0, 0);

            const element = this.getListItemLI(item);
            this.clonedElement = element.cloneNode(true);
            this.clonedElement.id = "cloned";
            this.clonedElement.style.transition = "all .15s";
            this.clonedElement.style.top = `${e.clientY}px`;
            this.clonedElement.style.left = `${e.clientX}px`;
            this.clonedElement.classList.add("dnn-persona-bar-treeview-dragged");

            document.body.appendChild(this.clonedElement);

            this.props._traverse((li, list, updateStore) => {
                li.selected = false;
                if (li.id === item.id) {
                    li.selected = true;
                    li.isOpen=false;
                    this.setState({ draggedItem: li, pageList: list, activePage: item }, ()=>updateStore(list));
                }
            });
        };

        const right = ()  => {
            this.props.showCancelDialog(item.id);
        };

        (!this.props.selectedPageDirty) ? left() : right();
    }

    onDrag(e) {
        const elm = this.clonedElement;
        elm.style.top = `${e.clientY}px`;
        elm.style.left = `${e.clientX - 30}px`;
        elm.style.transform = "rotate('5deg')";
    }

    onDragEnd(item) {
        this._fadeInTooltips();

        let pageList = null;
        let runUpdateStore = null;
        this.removeClone();
        this.props._traverse((item, list, updateStore) => {
            item.onDragOverState = false;
            pageList = list;
            runUpdateStore = updateStore;
        });
        this.setState({ pageList }, () => runUpdateStore(pageList) );
    }

    onDragLeave(item) {
        let pageList = null;
        this.props._traverse((pageListItem, list, updateStore) => {
            if (pageListItem.id === item.id) {
                pageListItem.onDragOverState = false;
                pageList = list;
                this.setState({ pageList: pageList }, () => updateStore(pageList));
            }
        });
    }

    onDragOver(e, item) {

        e.preventDefault();
        let pageList = null;
        this.props._traverse((pageListItem, list, updateStore) => {
            pageListItem.onDragOverState = false;

            if (pageListItem.id === item.id) {
                pageListItem.onDragOverState = true;
                pageList = list;
                this.setState({ pageList: pageList, dragOverItem: item }, ()=>updateStore(pageList));
            }
        });
    }

    onDrop(item, e) {
        this._fadeInTooltips();
        this.removeClone();


        const left = () => {
            let activePage = Object.assign({}, this.state.activePage);
            let pageList = null;
            let runUpdateStore = null;
            this.props._traverse((pageListItem, list, updateStore) => {
                pageListItem.onDragOverState = false;
                pageList = list;
                runUpdateStore = updateStore;
            });
            this.setState({ pageList }, ()=>runUpdateStore(pageList));

            this.getPageInfo(activePage.id)
                .then((data) => {
                    let activePage = Object.assign({}, this.state.activePage);
                    activePage.oldParentId = activePage.parentId;
                    activePage.parentId = item.id;
                    return this.props.saveDropState(activePage);
                })
                .then(this.getPageInfo.bind(this, activePage.id))
                .then(() => this.setState({ activePage: activePage, droppedItem: item } ));
        };

        const right = () => null;

        (item.id !== this.state.draggedItem.id) ? left() : right();
    }

    onMovePage({e, Action, PageId, ParentId, RelatedPageId }) {
        const { onMovePage } = this.props;

        onMovePage({ Action, PageId, ParentId, RelatedPageId })
            .then(() => this.removeDropZones())
            .then(() => this.reOrderPage({ Action, PageId, ParentId, RelatedPageId }));
    }

    removeClone() {
        this.clonedElement ? document.body.removeChild(this.clonedElement) : null;
        this.clonedElement = null;
    }

    removeDropZones() {
        return new Promise((resolve, reject) => {
            let pageList = null;
            let runUpdateStore = null;
            this.props._traverse((item, list, updateStore) => {
                item.onDragOverState = false;
                pageList = list;
                runUpdateStore = updateStore;
            });

            this.setState({ pageList }, () => {
                runUpdateStore(pageList);
                resolve();
            });
        });
    }

    reOrderPage({ Action, PageId, ParentId, RelatedPageId }) {
        return new Promise((resolve, reject) => {

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
                        case item.id === RelatedPageId:
                            newParentId = item.parentId;
                            break;

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

                this.setState({ pageList: pageList }, () => {
                    this.getPageInfo(cachedItem.id).then(() => {
                        cachedItem.url = `${window.origin}/${this.state.activePage.url}`;
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
                        case ParentId === -1:
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
                this.setState({ pageList }, () => {
                    runUpdateStore(pageList);
                    rez();
                });
            });

            removeFromPageList()
                .then(() => updateNewParent())
                .then(() => resolve());
        });
    }


    getChildListItems(id) {
        return new Promise((resolve, reject) => {
            const left = () => {
                const url = `${window.origin}/API/PersonaBar/${window.dnn.pages.apiController}/GetPageList?parentId=${id}`;

                this.GET(url)
                    .then((childListItems) => {
                        this.props._traverse((item, listItems, updateStore) => {
                            const left = () => item.childListItems = childListItems;
                            const right = () => null;
                            (item.id === id) ? left() : right();
                            this.setState({ pageList: listItems }, () => {
                                updateStore(listItems);
                                resolve();
                            });
                        });
                    });
            };

            const right = () => resolve();

            this._traverse((item) => (item.id === id && !item.hasOwnProperty('childListItems')) ? left() : right());
            this.toggleParentCollapsedState(id);

        });

    }

    addNewPageData(pageData) {
        const pageListArray = this.state.pageList.concat();
        const parentId = pageData.parentId;
    }


    toggleExpandAll() {
        let pageList = null;
        let runUpdateStore = null;
        const {isTreeviewExpanded} = this.state;

        this.props._traverse((item, list, updateStore) => {
            if (item.hasOwnProperty("childListItems") && item.childListItems.length > 0) {
                item.isOpen = (isTreeviewExpanded) ? false : true;
                updateStore(list);
                this.setState({isTreeviewExpanded:!this.state.isTreeviewExpanded});
            }
        });

    }


    render_treeview() {
        return (
            <span className="dnn-persona-bar-treeview-ul">
                {this.state.rootLoaded ?
                    <PersonaBarPageTreeview
                        draggedItem={this.state.draggedItem}
                        droppedItem={this.state.droppedItem}
                        dragOverItem={this.state.dragOverItem}

                        listItems={this.state.pageList}
                        getChildListItems={this.getChildListItems.bind(this)}
                        onSelection={this.onSelection.bind(this)}
                        onDrag={this.onDrag.bind(this)}
                        onDragStart={this.onDragStart.bind(this)}
                        onDragOver={this.onDragOver.bind(this)}
                        onDragLeave={this.onDragLeave.bind(this)}
                        onDragEnd={this.onDragEnd.bind(this)}
                        onDrop={this.onDrop.bind(this)}
                        onMovePage={this.onMovePage.bind(this)}
                        getPageInfo={this.getPageInfo.bind(this)}
                    />

                    : null}
            </span>
        );
    }

    render_treemenu() {
        return (
            <span className="dnn-persona-bar-treeview-ul">
                {this.state.rootLoaded ?
                    <PersonaBarPageTreeMenu listItems={this.state.pageList} />

                    : null}
            </span>
        );
    }

    render_tree_parent_expand() {
        return (
            <span className="dnn-persona-bar-treeview-ul">
                {this.state.rootLoaded ?
                    <PersonaBarPageTreeParentExpand listItems={this.state.pageList}  getChildListItems={this.getChildListItems.bind(this)}/>

                    : null}
            </span>
        );
    }

    render_collapseExpand() {
        return (
            <div onClick={this.toggleExpandAll.bind(this)} className={(this.state.initialCollapse) ? "collapse-expand initial" : "collapse-expand"}>
                [{this.state.isTreeviewExpanded ? "COLLAPSE ALL" : "EXPAND ALL"}]
            </div>
        );
    }

    render() {

        return (
            <GridCell columnSize={30} className="dnn-persona-bar-treeview">
                 {this.render_collapseExpand()}
                <GridCell columnSize={15}>
                    <div className="dnn-persona-bar-treeview-menu">
                         {this.render_tree_parent_expand()}
                    </div>
                </GridCell>
                <GridCell columnSize={55}>
                    <Scrollbars className="scrollArea content-horizontal"
                        autoHeight
                        autoHeightMin={0}
                        autoHeightMax={9999}>
                            {this.render_treeview()}
                    </Scrollbars>
                </GridCell>
                <GridCell columnSize={30}>
                    <div className="dnn-persona-bar-treeview-menu" style={{float:"right"}}>
                         {this.render_treemenu()}
                    </div>
                </GridCell>
            </GridCell>
        );
    }
}

PersonaBarPageTreeviewInteractor.propTypes = {
    _traverse: PropTypes.func.isRequired,
    showCancelDialog: PropTypes.func.showCancelDialog,
    selectedPageDirty: PropTypes.bool.isRequired,
    activePage: PropTypes.object.isRequired,
    onSelection: PropTypes.func.isRequired,
    onMovePage: PropTypes.func.isRequired,
    setActivePage: PropTypes.func.isRequired,
    saveDropState: PropTypes.func.isRequired

};

// <PersonaBarPageTreeMenu listItems={this.state.pageList} />