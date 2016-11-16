import React, {Component, PropTypes} from "react";
import "./style.less";

const folderIcon = require("!raw!./img/folder.svg");

export default class Folders extends Component {

    constructor() {
        super();
        this.state = {
            openFolders: []
        };
    }

    componentWillReceiveProps(props) {
        if (!props.folders || !props.folders.children || !props.folders.children[0] || 
            !props.folders.children[0].data || !props.folders.children[0].data.key) {
            return;
        }
        const rootKey = props.folders.children[0].data.key;
        const {openFolders} = this.state;
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

    onParentClick(item) {
        if (!item.data.hasChildren) {
            return;
        }
        this.toggleFolder(item.data.key);
        if (item.children && item.children.length) {
            return;
        }
        this.props.onParentExpands(item.data.key);
    }

    onFolderNameClick(folder) {
        this.props.onFolderChange(folder.data);
    }

    getFolderIcon() {
        /* eslint-disable react/no-danger */
        return (<div className="icon" dangerouslySetInnerHTML={{ __html: folderIcon }} />);
        /* eslint-enable react/no-danger */
    }

    getFolders(folder) {
        if (!folder) {
            return false;
        }

        const folderIcon = this.getFolderIcon();
        const children = folder.children.map((child) => {
            const isOpen = this.state.openFolders.some(id => id === child.data.key);
            const className = isOpen ? "open" : "";
            return <li className={className}>
                {child.data.hasChildren && 
                    <div 
                        className="has-children" 
                        onClick={this.onParentClick.bind(this, child)}>
                    </div>
                }
                <div onClick={this.onFolderNameClick.bind(this, child)}>
                    {folderIcon}
                    <div className="item-name">{child.data.value}</div>
                </div>
                {child.data.hasChildren && 
                    this.getFolders(child) 
                }
            </li>;
        });

        return <ul>{children}</ul>;
    }

    render() {
        const folders = this.getFolders(this.props.folders);

        return (
            <div className="dnn-folders-component">
                {folders}
            </div>
        );
    }
}

Folders.propTypes = {
    folders: PropTypes.object.isRequired,
    onFolderChange: PropTypes.func.isRequired,
    onParentExpands: PropTypes.func.isRequired
};