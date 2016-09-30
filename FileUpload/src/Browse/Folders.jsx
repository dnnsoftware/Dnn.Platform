import React, {Component, PropTypes} from "react";
const folderIcon = require("!raw!./img/folder.svg");

export default class Folders extends Component {

    constructor() {
        super();
        this.state = {
            openFolders: []
        };
    }

    componentWillReceiveProps(props) {
        if (!props.folders || !props.folders.children || !props.folders.children[0] || !props.folders.children[0].data || !props.folders.children[0].data.key) {
            return;
        }
        const rootKey = props.folders.children[0].data.key;
        let {openFolders} = this.state;
        openFolders.push(rootKey);
        this.setState({ openFolders });
    }


    toggleFolder(key) {
        let {openFolders} = this.state;
        if (openFolders.some(id => id === key)) {
            openFolders = openFolders.filter(id => id !== key);
        } else {
            openFolders.push(key);
        }
        this.setState({ openFolders });
    }

    onParentClick(item, e) {
        if (!item.data.hasChildren) {
            return;
        }
        this.toggleFolder(item.data.key);
        if (item.children && item.children.length) {
            return;
        }
        this.props.getChildren(item.data.key);
    }

    isMatchSearch(folderName) {
        if (!this.props.searchFolderText) {
            return true;
        }
        return folderName.indexOf(this.props.searchFolderText) !== -1;
    }

    onFolderNameClick(folder, e) {
        this.props.onFolderClick(folder.data);
    }

    getFolders(folder) {
        if (!folder) {
            return false;
        }
        const children = folder.children.map((child) => {
            /* eslint-disable react/no-danger */
            const isOpen = this.state.openFolders.some(id => id === child.data.key);
            const isMatchSearch = this.isMatchSearch(child.data.value);
            const className = isOpen ? "open" : "";
            return <li className={className}>
                {isMatchSearch && child.data.hasChildren && <div className="has-children" onClick={this.onParentClick.bind(this, child) }></div>}
                {isMatchSearch && <div className="icon" dangerouslySetInnerHTML={{ __html: folderIcon }} onClick={this.onParentClick.bind(this, child) }/>}
                {isMatchSearch && <div className="item-name" onClick={this.onFolderNameClick.bind(this, child)}>{child.data.value}</div>}
                {child.data.hasChildren && this.getFolders(child) }
            </li>;
        });
        return <ul>{children}</ul>;
    }

    render() {
        /* eslint-disable react/no-danger */
        const folders = this.getFolders(this.props.folders);
        return <div className="item-picker">
            {folders}
        </div>;
    }
}


Folders.propTypes = {
    folders: PropTypes.object.isRequired,
    onFolderClick: PropTypes.func.isRequired,
    getChildren: PropTypes.func.isRequired,
    searchFolderText: PropTypes.string.isRequired
};

