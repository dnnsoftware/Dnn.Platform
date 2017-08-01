import React, { Component } from "react";
import { PersonaBarPageTreeview } from "./PersonaBarPageTreeview";
import { PropTypes } from "prop-types";
import Promise from "promise";
import GridCell from "dnn-grid-cell";
import "./styles.less";


export class PersonaBarPageTreeviewInteractor extends Component {

    constructor() {
        super();
        this.state = {
            rootLoaded: false,
            isTreeviewExpanded: false
        };
        this.origin = window.origin;

        this.getRootListItems();
    }

    componentDidMount() {
        this.init();

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

    getRootListItems() {
        const url = `${window.origin}/API/PersonaBar/${window.dnn.pages.apiController}/GetPageList?searchKey=`;
        this.GET(url).then((data) => {

            this.setState({ pageList: data, isTreeviewExpanded: true, rootLoaded: true });
        });
    }

    toggleParentCollapsedState(id) {
        this._traverse((item, listItem) => {
            (item.id === id) ? item.isOpen = !item.isOpen : null;
            this.setState({ pageList: listItem });
        });
    }

    onSelection(id) {
        this._traverse((item, listItem) => {
            (item.id === id && item.canViewPage) ? item.selected = true : item.selected = false;
            this.setState({ pageList: listItem }, ()=>{
                item.selected ?  this.props.onSelection(id) : null;
            });

        });
    }

    getListItemLI(item) {
        return document.getElementById(`list-item-${item.name}-${item.id}`);
    }

    getListItemTitle(item) {
        return document.getElementById(`list-item-title-${item.name}-${item.id}`);
    }

    onDragStart(e, item) {
        const img = new Image();
        e.dataTransfer.setDragImage(img, 0, 0);

        const element = this.getListItemLI(item);
        this.clonedElement = element.cloneNode(true);
        this.clonedElement.id = "cloned";
        this.clonedElement.style.transition = "all";
        this.clonedElement.classList.add("dnn-persona-bar-treeview-dragged");

        document.body.appendChild(this.clonedElement);

        this._traverse((li, list) => {
            li.selected = false;
            if (li.id === item.id) {
                li.selected = true;
                this.setState({ draggedItem: li, pageList: list, activePage: item }, ()=>{

                });
            }
        });
    }

    onDrag(e) {
        const elm = this.clonedElement;
        elm.style.top = `${e.clientY}px`;
        elm.style.left = `${e.clientX - 30}px`;
    }

    onDragEnd(item) {
        let pageList = null;
        this.removeClone();
        this._traverse((item, list) => {
            item.onDragOverState = false;
            pageList = list;
        });
        this.setState({ pageList });
    }

    onDragLeave(item) {
        let pageList = null;
        this._traverse((pageListItem, list) => {
            if (pageListItem.id === item.id) {
                pageListItem.onDragOverState = false;
                pageList = list;
            }
        });
        this.setState({ pageList: pageList });
    }

    onDragOver(e, item) {
        e.preventDefault();
        let pageList = null;
        this._traverse((pageListItem, list) => {
            pageListItem.onDragOverState = false;

            if (pageListItem.id === item.id) {
                pageListItem.onDragOverState = true;
                pageList = list;
            }
        });
        this.setState({ pageList: pageList, dragOverItem: item });
    }

    onDrop(item) {
        this.removeClone();
        let activePage = Object.assign({}, this.state.activePage);
        let pageList = null;

        this._traverse((pageListItem, list) => {
            pageListItem.onDragOverState = false;
            pageList = list;
        });
        this.setState({ pageList });

        this.getPageInfo(activePage.id)
            .then((data) => {
                let activePage = Object.assign({}, this.state.activePage);
                activePage.parentId = item.id;
                return this.props.saveDropState(activePage);
            })
            .then(this.getPageInfo.bind(this, activePage.id))
            .then(() => this.setState({ activePage: activePage, droppedItem: item }, () => this.updateTree()));

    }

    onMovePage({ Action, PageId, ParentId, RelatedPageId }) {
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
            this._traverse((item, list) => {
                item.onDragOverState = false;
                pageList = list;
            });

            this.setState({ pageList }, () => {
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

            const removeFromPageList = () => new Promise((rez) => {
                this._traverse((item, list) => { // remove item from pagelist and cache

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
                        rez();
                    });
                });
            });


            const updateNewParent = () => new Promise((rez) => {
                this._traverse((item, list) => {
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
                this.setState({ pageList }, () => rez());
            });

            removeFromPageList()
                .then(() => updateNewParent())
                .then(() => resolve());
        });
    }

    updateTree() {
        const newParent = this.state.droppedItem;
        const moveChild = this.state.draggedItem;

        const condition = (newParent.id != moveChild.parentId);

        const popMoveChildItem = () => {
            return new Promise((resolve, reject) => {
                let update = null;
                this._traverse((item, list) => {
                    let cachedItemIndex;
                    let cachedItemIndexParent;

                    const left = () => {
                        item.childListItems.filter((data, index) => {
                            if (data.id === moveChild.id) {
                                cachedItemIndex = index;
                            }
                        });
                        const arr1 = item.childListItems.slice(0, cachedItemIndex);
                        const arr2 = item.childListItems.slice(cachedItemIndex + 1);
                        item.childListItems = [...arr1, ...arr2];
                        item.childCount--;
                        update = list;
                    };

                    const right = () => {
                        let rootList = list.concat();
                        rootList.filter((item, index) => {
                            if (item.id === moveChild.id) {
                                cachedItemIndex = index;
                                const arr1 = rootList.slice(0, cachedItemIndex);
                                const arr2 = rootList.slice(cachedItemIndex + 1);
                                rootList = [...arr1, ...arr2];
                                update = rootList;

                            }
                        });
                    };

                    switch (true) {
                        case item.id === moveChild.parentId:
                            left();
                            return;
                        case moveChild.parentId === -1:
                            right();
                            return;
                        default:

                    }

                });

                this.setState({ pageList: update }, () => {
                    resolve();
                });

            });
        };

        const insertMoveChild = () => {
            this._traverse((item, list) => {
                const left = () => {
                    moveChild.parentId = item.id;
                    item.childCount++;
                    item.childListItems = (Array.isArray(item.childListItems)) ? item.childListItems : [];
                    item.childListItems.push(moveChild);
                    this.setState({ pageList: list });
                };
                const right = () => {
                    this.getChildListItems(item.id)
                        .then(() => {

                            if (item.id === newParent.id) {
                                moveChild.parentId = item.id;
                                item.isOpen = true;
                                item.childCount++;
                                item.childListItems.push(moveChild);
                                this.setState({ pageList: list });
                            }

                        });
                };

                if (item.id === newParent.id) {
                    (item.childCount === 0) ? left() : right();
                }

            });
        };

        popMoveChildItem().then(() => insertMoveChild());

    }

    getChildListItems(id) {
        return new Promise((resolve, reject) => {
            const left = () => {
                const url = `${window.origin}/API/PersonaBar/${window.dnn.pages.apiController}/GetPageList?parentId=${id}`;

                this.GET(url)
                    .then((childListItems) => {
                        this._traverse((item, listItems) => {
                            const left = () => item.childListItems = childListItems;
                            const right = () => null;
                            (item.id === id) ? left() : right();
                            this.setState({ pageList: listItems }, () => {
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
        const {isTreeviewExpanded} = this.state;

        this._traverse((item, list) => {
            if (item.hasOwnProperty("childListItems") && item.childListItems.length > 0) {
                item.isOpen = isTreeviewExpanded ?  item.isOpen=false : item.isOpen=true ;
                pageList = list;
            }
        });


        if (pageList) {
            this.setState({ pageList: pageList, isTreeviewExpanded: !this.state.isTreeviewExpanded });
        }

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

    render_collapseExpand() {
        return (
            <div onClick={this.toggleExpandAll.bind(this)} className="collapse-expand">
                [{this.state.isTreeviewExpanded ? "COLLAPSE" : "EXPAND"}]
            </div>
        );
    }

    render() {

        return (
            <GridCell columnSize={30} className="dnn-persona-bar-treeview">
                {this.render_collapseExpand()}
                {this.render_treeview()}

            </GridCell>
        );
    }
}

PersonaBarPageTreeviewInteractor.propTypes = {
    activePage: PropTypes.object.isRequired,
    onSelection: PropTypes.func.isRequired,
    onMovePage: PropTypes.func.isRequired,
    setActivePage: PropTypes.func.isRequired,
    saveDropState: PropTypes.func.isRequired

};