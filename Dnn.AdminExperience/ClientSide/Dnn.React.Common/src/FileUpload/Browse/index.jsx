import React, { Component } from "react";
import PropTypes from "prop-types";
import FolderPicker from "./FolderPicker";
import FilePicker from "./FilePicker";
import helper from "../helper";

import "./style.less";
const KEY = {
    ENTER: 13,
    ESCAPE: 27
};

function findKey(thisObject, id) {
    let p, tRet;
    for (p in thisObject) {
        if (p === "data") {
            if (thisObject[p].key === id) {
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

    componentDidUpdate(prevProps) {
        if (this.props.portalId !== prevProps.portalId) {
            this.getFolders();
        }
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
        let portalIdParams = this.props.portalId && this.props.portalId !== -1
            ? { portalId: this.props.portalId }
            : {};
                    
        if (!searchText) {
            return sf.get("GetFolders", portalIdParams, this.setFolders.bind(this), this.handleError.bind(this));
        }
        sf.get("SearchFolders", { searchText }, this.setFolders.bind(this), this.handleError.bind(this));
    }

    getChildrenFolders(parentId) {
        const sf = this.getServiceFramework();
        sf.get("GetFolderDescendants", { parentId }, this.addChildFolders.bind(this, parentId), this.handleError.bind(this));
    }

    getExtensions(fileFormats) {
        let result = "";
        
        const extensions = {
            "text/html": "htm,html",
            "text/css": "css",
            "text/xml": "xml",
            "image/gif": "gif",
            "image/jpeg": "jpeg,jpg",
            "application/x-javascript": "js",
            "text/plain": "txt",
            "image/png": "png",
            "image/tiff": "tif,tiff",
            "image/x-icon": "ico",
            "image/x-ms-bmp": "bmp",
            "image/svg+xml": "svg",
            "image/webp": "webp",
            "application/msword": "doc",
            "application/pdf": "pdf",
            "application/rtf": "rtf",
            "application/vnd.ms-excel": "xls",
            "application.vnd.ms-powerpoint": "ppt",
            "application/x-shockwave-flash": "swf",
            "appliation/zip": "zip",
            "audio/mpeg": "mp3",
            "audio/ogg": "ogg",
            "video/3gpp": "3gpp,3gp",
            "video/mpeg": "mpeg,mpg",
            "video/quicktime": "mov",
            "video/x-flv": "flv",
            "video/x-ms-wmv": "wmv",
            "video/x-msvideo": "avi",
            "video/mp4": "m4v,mp4",
        };

        fileFormats.map(fileFormat => {
            if (extensions[fileFormat]) {
                result += extensions[fileFormat] + ",";
            }
            else {
                result += fileFormat.split("/")[1] + ",";
            }
        });
        return result;
    }

    getFiles() {
        const sf = this.getServiceFramework();
        let parentId = this.state.selectedFolder ? this.state.selectedFolder.key : null;
        const extensions = this.getExtensions(this.props.fileFormats);
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
        const extensions = this.getExtensions(this.props.fileFormats);
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
        if (this.state.selectedFile && !this.state.selectedFolder || this.state.selectedFolder && this.state.selectedFolder.value === "0") {
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

    renderActions() {
        const {props} = this;

        return helper.renderActions(props.browseActionText, {
            "save": this.onSave.bind(this),
            "cancel": props.onCancel
        });
    }

    render() {
        /* eslint-disable react/no-danger */
        return <div className="file-upload-container">
            <h4>{this.props.folderText}</h4>
            <FolderPicker
                notSpecifiedText={this.props.notSpecifiedText}
                searchFoldersPlaceHolderText={this.props.searchFoldersPlaceHolderText}
                selectedFolder={this.state.selectedFolder}
                folders={this.state.folders}
                searchFolder={this.getFolders.bind(this)}
                onFolderClick={this.onFolderClick.bind(this) }
                getChildren={this.getChildrenFolders.bind(this) }
            />
            <h4>{this.props.fileText}</h4>
            <FilePicker
                notSpecifiedText={this.props.notSpecifiedText}
                searchFilesPlaceHolderText={this.props.searchFilesPlaceHolderText}
                selectedFile={this.state.selectedFile}
                files={this.state.files}
                onFileClick={this.onFileClick.bind(this) }
                getFiles={this.getFiles.bind(this) }                
            />
            <span>{this.renderActions()}</span>
        </div>;
    }
}


Browse.propTypes = {    
    utils: PropTypes.object.isRequired,
    selectedFile: PropTypes.object.isRequired,
    selectedFolder: PropTypes.object.isRequired,
    fileFormats: PropTypes.array,
    onSave: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired,

    portalId: PropTypes.number,
    browseActionText: PropTypes.string,
    fileText: PropTypes.string,
    folderText: PropTypes.string,
    notSpecifiedText: PropTypes.string,
    searchFoldersPlaceHolderText: PropTypes.string,
    searchFilesPlaceHolderText: PropTypes.string
};

Browse.defaultProps = {
    fileFormats: []
};

