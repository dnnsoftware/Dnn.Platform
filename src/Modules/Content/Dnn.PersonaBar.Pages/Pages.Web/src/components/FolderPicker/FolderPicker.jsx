import React, {Component, PropTypes} from "react";
import FolderPickerContainer from "./FolderPickerContainer";

function findKey(thisObject, id) {
    /* eslint-disable spellcheck/spell-checker */
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
    /* eslint-enable spellcheck/spell-checker */
}

export default class FolderPicker extends Component {

    constructor(props) {
        super(props);

        this.state = {
            folders: null
        };
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
        const {selectedFolder, onSelectFolder} = this.props;

        return (
            <FolderPickerContainer
                selectedFolder={selectedFolder}
                folders={this.state.folders}
                searchFolder={this.getFolders.bind(this)}
                onFolderClick={onSelectFolder}
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
