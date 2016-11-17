import React, {Component, PropTypes} from "react";
import FolderPicker from "./FolderPicker";
import FilePicker from "./FilePicker";

import "./style.less";
const KEY = {
    ENTER: 13,
    ESCAPE: 27
};

function findKey(thisObject, id) {
    let p, tRet;
    for (p in thisObject) {
        if (p == "data") {
            if (thisObject[p].key == id) {
                return thisObject;
            }
        } else if (thisObject[p] instanceof Object) {
            if (thisObject.hasOwnProperty(p)) {
                tRet = findKey(thisObject[p], id);
                if (tRet) { return tRet; }
            }
        }
    }
    return false;
}

export default class Browse extends Component {

    constructor(props) {
        super(props);

        let selectedFolder = this.props.selectedFolder;
        this.state = {
            showFolderPicker: false,
            showFilePicker: false,
            folders: null,
            files: null,
            selectedFolder,
            selectedFile: this.props.selectedFile
        };
        this.onKeyDown = this.onKeyDown.bind(this);
    }

    onKeyDown(event) {
        switch (event.keyCode) {
            case KEY.ENTER:
                return this.props.onSave(this.state.selectedFolder, this.state.selectedFile);
            case KEY.ESCAPE:
                return this.props.onCancel();
        }
    }

    onSave() {
        return this.props.onSave(this.state.selectedFolder, this.state.selectedFile);
    }

    componentWillUnmount() {
        document.removeEventListener("keyup", this.onKeyDown, false);
    }

    componentDidMount() {
        this.getFolders();
        document.addEventListener("keyup", this.onKeyDown, false);
    }

    getServiceFramework() {
        let sf = this.props.utils.utilities.sf;
        sf.controller = "ItemListService";
        sf.moduleRoot = "InternalServices";
        return sf;
    }

    getFolders(searchText) {
        const sf = this.getServiceFramework();
        if (!searchText) {
            return sf.get("GetFolders", {}, this.setFolders.bind(this), this.handleError.bind(this));
        }
        sf.get("SearchFolders", { searchText }, this.setFolders.bind(this), this.handleError.bind(this));
    }

    getChildrenFolders(parentId) {
        const sf = this.getServiceFramework();
        sf.get("GetFolderDescendants", { parentId }, this.addChildFolders.bind(this, parentId), this.handleError.bind(this));
    }

    getFiles() {
        const sf = this.getServiceFramework();
        let parentId = this.state.selectedFolder ? this.state.selectedFolder.key : null;
        const extensions = this.props.fileFormats.map(format => format.split("/")[1]).join(",");
        if (parentId) {
            sf.get("GetFiles", { parentId, filter: extensions }, this.setFiles.bind(this), this.handleError.bind(this));
        } else if (this.state.selectedFolder) {
            sf.get("SearchFolders", { searchText: this.state.selectedFolder.value }, this.setFolderId.bind(this), this.handleError.bind(this));
        } else {
            this.setFiles();
        }
    }

    setFolderId(result) {
        const selectedFolder = result.Tree.children[0].data;
        const extensions = this.props.fileFormats.map(format => format.split("/")[1]).join(",");
        const sf = this.getServiceFramework();
        sf.get("GetFiles", { parentId: selectedFolder.key, filter: extensions }, this.setFiles.bind(this), this.handleError.bind(this));
    }

    setFiles(result) {
        if (!result || !result.Tree || !result.Tree.children) {
            return;
        }
        this.setState({ files: result.Tree.children });
    }

    setFolders(result) {
        this.setState({ folders: result.Tree });
        if (this.state.selectedFile && !this.state.selectedFolder || this.state.selectedFolder && this.state.selectedFolder.value == "0") {
            const selectedFolder = result.Tree.children[0].data;
            this.setState({ selectedFolder });
        }
    }

    handleError() {

    }

    onFolderClick(folder) {
        this.setState({ selectedFolder: folder, files: null, selectedFile: null });
    }

    onFileClick(file) {
        const selectedFile = file ? file.data : null;
        this.setState({ selectedFile });
    }

    addChildFolders(parentId, result) {
        let folders = this.state.folders;
        let parent = findKey(folders, parentId);
        let children = result.Items.map((item) => {
            return { data: item, children: [] };
        });
        parent.children = children;
        this.setState({ folders });
    }

    render() {
        /* eslint-disable react/no-danger */
        return <div className="file-upload-container">
            <h4>Folder</h4>
            <FolderPicker
                selectedFolder={this.state.selectedFolder}
                folders={this.state.folders}
                searchFolder={this.getFolders.bind(this)}
                onFolderClick={this.onFolderClick.bind(this) }
                getChildren={this.getChildrenFolders.bind(this) }/>
            <h4>File</h4>
            <FilePicker
                selectedFile={this.state.selectedFile}
                files={this.state.files}
                onFileClick={this.onFileClick.bind(this) }
                getFiles={this.getFiles.bind(this) }
                />
            <span>Press <strong onClick={this.onSave.bind(this) }>[ENTER]</strong> to save, or <strong onClick={this.props.onCancel}>[ESC]</strong> to cancel</span>
        </div>;
    }
}


Browse.propTypes = {
    utils: PropTypes.object.isRequired,
    selectedFile: PropTypes.object.isRequired,
    selectedFolder: PropTypes.object.isRequired,
    fileFormats: PropTypes.array,
    onSave: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired
};

Browse.defaultProps = {
    fileFormats: []
};

