import React, {Component, PropTypes} from "react";
import FolderPickerContainer from "./FolderPickerContainer";

function findKey(thisObject, id) {
    /* eslint-disable react/no-danger */
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
    /* eslint-enable react/no-danger */
}

export default class FolderPicker extends Component {

    constructor(props) {
        super(props);

        this.state = {
            showFolderPicker: false,
            showFilePicker: false,
            folders: null
        };
    }

    componentWillMount() {
        const {selectedFolder} = this.props;
        this.setState({ selectedFolder });
    }

    componentDidMount() {
        this.getFolders();
    }

    getServiceFramework() {
        const sf = this.props.serviceFramework;
        sf.controller = "ItemListService";
        sf.moduleRoot = "InternalServices";
        return sf;
    }

    getFolders(searchText) {
        const sf = this.getServiceFramework();
        if (!searchText) {
            return sf.get("GetFolders", {}, this.setFolders.bind(this), this.props.onServiceError);
        }
        sf.get("SearchFolders", { searchText }, this.setFolders.bind(this), this.props.onServiceError);
    }

    getChildrenFolders(parentId) {
        const sf = this.getServiceFramework("Vocabularies");
        sf.get("GetFolderDescendants", { parentId }, this.addChildFolders.bind(this, parentId), this.props.onServiceError);
    }

    setFolders(result) {
        this.setState({ folders: result.Tree });
    }

    onFolderClick(selectedFolder) {
        this.setState({ selectedFolder });
        this.props.onSelectFolder(selectedFolder);
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
        return (
            <FolderPickerContainer
                selectedFolder={this.state.selectedFolder}
                folders={this.state.folders}
                searchFolder={this.getFolders.bind(this)}
                onFolderClick={this.onFolderClick.bind(this) }
                getChildren={this.getChildrenFolders.bind(this) }/>
        );    
    }
}

FolderPicker.propTypes = {
    serviceFramework: PropTypes.object.isRequired,
    selectedFolder: PropTypes.object.isRequired,
    onSelectFolder: PropTypes.func.isRequired,
    onServiceError: PropTypes.func.isRequired
};
