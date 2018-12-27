import React, { Component } from "react";
import PropTypes from "prop-types";
import resx from "resources";
import { Scrollbars } from "react-custom-scrollbars";
import Folder from "./folder";
import { GridCell, SvgIcons, Label, Collapsible } from "@dnnsoftware/dnn-react-common";
const parentTermTreeStyle = {
    width: "100%",
    height: "250px",
    border: "1px solid #BFBFBF",
    background: "white",
    boxSizing: "border-box"
};

/*eslint-disable react/no-danger*/
function mapChildFolders(folders, getChildFolders, getResxEntries, resxBeingEdited) {
    return folders.map((folder, i) => {
        const isResxFile = folder.NewValue.indexOf(".resx") !== -1;

        return <Folder
            onClick={(!isResxFile ? getChildFolders.bind(this) : getResxEntries.bind(this))}
            folder={folder}
            ChildFolders={folder.ChildFolders}
            isSelected={resxBeingEdited === folder.NewValue}
            key={i}>
            {(folder.ChildFolders && folder.ChildFolders.length > 0) && mapChildFolders(folder.ChildFolders, getChildFolders, getResxEntries, resxBeingEdited)}
        </Folder>;
    });
}

class ResourceTree extends Component {

    constructor() {
        super();
        this.state = {
            treeOpened: false
        };
        this.handleClick = this.handleClick.bind(this);
    }

    componentDidMount() {
        document.addEventListener("click", this.handleClick, false);
        this._isMounted = true;
    }

    componentWillUnmount() {
        document.removeEventListener("click", this.handleClick, false);
        this._isMounted = false;
    }

    handleClick(e) {
        if (!this._isMounted) { return; }
        const node = this.node;
        if (node && node.contains(e.target)) {
            return;
        }
        this.hide();
    }
    hide() {
        this.setState({ treeOpened: false });
    }
    onToggleTree() {
        this.setState({
            treeOpened: !this.state.treeOpened
        });
    }
    render() {
        const {
            languageFolders,
            getChildFolders,
            getResxEntries,
            resxBeingEdited,
            resxBeingEditedDisplay
        } = this.props;
        return (
            <GridCell columnSize={100} className="resource-file-tree-container" ref={node => this.node = node}>
                <Label label={resx.get("ResourceFile")} />
                <div className="resource-file-dropdown" onClick={this.onToggleTree.bind(this)} style={{ width: "50%" }}>
                    {resxBeingEditedDisplay || resx.get("SelectResourcePlaceholder")}
                    <div className="dropdown-icon" dangerouslySetInnerHTML={{ __html: SvgIcons.ArrowDownIcon }}></div>
                </div>
                <Collapsible isOpened={this.state.treeOpened} className="tree-container" keepCollapsedContent={true}>
                    <Scrollbars style={parentTermTreeStyle}>
                        <ul className="resource-tree root-level parent-tree">
                            {mapChildFolders(languageFolders, getChildFolders, getResxEntries, resxBeingEdited)}
                        </ul>
                    </Scrollbars>
                </Collapsible>
            </GridCell>
        );
    }
}

ResourceTree.propTypes = {
    languageBeingEdited: PropTypes.object,
    ModeOptions: PropTypes.array,
    languageFolders: PropTypes.array,
    languageFiles: PropTypes.array,
    getChildFolders: PropTypes.func,
    getResxEntries: PropTypes.func,
    resxBeingEdited: PropTypes.string,
    onToggleTree: PropTypes.func,
    resxBeingEditedDisplay: PropTypes.string
};

export default ResourceTree;