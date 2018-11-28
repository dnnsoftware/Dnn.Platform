import React, { Component } from "react";
import PropTypes from "prop-types";
import Collapse from "dnn-collapsible";
import { FolderIcon, PagesIcon, ArrowRightIcon, ArrowDownIcon } from "dnn-svg-icons";
class Folder extends Component {
    constructor() {
        super();
        this.state = {
            isOpen: false
        };
    }
    toggleCollapse() {
        const { props } = this;
        this.setState({
            isOpen: !this.state.isOpen
        });
        if (!props.children) {
            props.onClick(props.folder.NewValue, () => {
                this.setState({ isOpen: true });
            });
        }
    }
    /*eslint-disable react/no-danger*/
    render() {
        const { props } = this;
        const isFolder = props.folder.NewValue.indexOf(".resx") === -1;
        const svgIcon = isFolder ? FolderIcon : PagesIcon;
        const isOpenIcon = this.state.isOpen ? ArrowDownIcon : ArrowRightIcon;
        return (
            <li>
                {isFolder && <div onClick={this.toggleCollapse.bind(this)} className="edit-svg" dangerouslySetInnerHTML={{ __html: isOpenIcon }}></div>}
                <div className="resource-type-icon" onClick={this.toggleCollapse.bind(this)} dangerouslySetInnerHTML={{ __html: svgIcon }}></div>
                <span className={props.isSelected ? "selected-resource" : ""} onClick={this.toggleCollapse.bind(this)}>{props.folder.Name}</span>
                <Collapse isOpened={this.state.isOpen} keepCollapsedContent={true}>
                    <ul>{props.children}</ul>
                </Collapse>
            </li>
        );
    }
}

Folder.propTypes = {
    ChildFolders: PropTypes.array,
    getChildFolders: PropTypes.func,
    folder: PropTypes.object
};
export default Folder;
