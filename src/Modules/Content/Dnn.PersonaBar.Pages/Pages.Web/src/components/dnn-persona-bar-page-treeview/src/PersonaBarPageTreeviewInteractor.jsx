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
            isTreeviewExpanded: false
        };
        this.origin = window.origin;
        this.getRootListItems();
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

    getRootListItems() {
        const url = `${window.origin}/API/PersonaBar/${window.dnn.pages.apiController}/GetPageList?searchKey=`;
        this.GET(url)
            .then((data) => this.setState({ pageList: data }));
    }

    toggleParentCollapsedState(id){
        this._traverse((item, listItem)=>{
            (item.id===id)  ? item.isOpen = !item.isOpen : null;
            this.setState({pageList:listItem});
        });
    }

    onSelection(id){
        console.log('in selection');
        this._traverse((item, listItem) => {
           (item.id===id) ? item.selected=true : item.selected=false;
           this.setState({pageList: listItem });
        });
    }

    getChildListItems(id) {

        const left = () => {
            const url = `${window.origin}/API/PersonaBar/${window.dnn.pages.apiController}/GetPageList?parentId=${id}`;

            this.GET(url)
                .then((childListItems)=> {
                    this._traverse((item, listItems) => {
                        const left = () => item.childListItems=childListItems;
                        const right = () => null;
                        (item.id === id ) ? left() : right();
                        this.setState({pageList:listItems});
                    });
                });
        };

        const right = () => null;

        this._traverse((item) => (item.id === id && !item.hasOwnProperty('childListItems')) ? left() : right());
        this.toggleParentCollapsedState(id);
    }

    addNewPageData(pageData) {
        const pageListArray = this.state.pageList.concat();
        const parentId = pageData.parentId;
    }


    toggleTreeview() {
        this.setState({
            isTreeviewExpanded: !this.state.isTreeviewExpanded
        });
    }

    render_treeview() {
        return (
            <span className="dnn-persona-bar-treeview-ul">
                { this.state.isTreeviewExpanded ?
                    <PersonaBarPageTreeview
                        listItems={this.state.pageList}
                        getChildListItems={this.getChildListItems.bind(this)}
                        onSelection={this.onSelection.bind(this)}
                        />
                : null}
            </span>
        );
    }

    render_collapseExpand() {
        return (
            <div onClick={this.toggleTreeview.bind(this)} className="collapse-expand">
              [{this.state.isTreeviewExpanded ? "COLLAPSE" : "EXPAND" }]
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
    OnSelect: PropTypes.func.isRequired
};