import React, {Component, PropTypes} from "react";
import FolderSelector from "./FolderSelector";

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
        const portalId = `${this.props.portalId === -1 ? "" : "?portalId=" + this.props.portalId}`;
        if (!searchText) {
            return sf.get("GetFolders" + portalId, {}, this.setFolders.bind(this), this.props.onRetrieveFolderError);
        }
        sf.get("SearchFolders" + portalId, { searchText }, this.setFolders.bind(this), this.props.onRetrieveFolderError);
    }

    getChildrenFolders(parentId) {
        const sf = this.getServiceFramework();
        sf.get(`GetFolderDescendants${this.props.portalId === -1 ? "" : "?portalId=" + this.props.portalId}`, { parentId }, this.addChildFolders.bind(this, parentId), this.props.onRetrieveFolderError);
    }

    setFolders(result) {
        this.setState({ folders: result.Tree });
    }

    addChildFolders(parentId, result) {
        const folders = this.state.folders;
        const parent = findKey(folders, parentId);
        const children = result.Items.map((item) => {
            return { data: item, children: [] };
        });
        parent.children = children;
        this.setState({ folders });
    }

    render() {
        const {onSelectFolder} = this.props;
        
        return (
            <FolderSelector
                folders={this.state.folders}
                searchFolder={this.getFolders.bind(this)}
                onParentExpands={this.getChildrenFolders.bind(this)}
                onFolderChange={onSelectFolder}
                {...this.props} />
        );    
    }
}

/**
 * propTypes
 * @property {object} serviceFramework service to retrieve data
 * @property {object} selectedFolder selected folder. Should be null or have a key and value properties
 * @property {object} onSelectFolder called when change folder. Pass as parameter a selectedFolder object
 * @property {object} onRetrieveFolderError callback for error when retrieve data
 */

FolderPicker.propTypes = {
    serviceFramework: PropTypes.object.isRequired,
    selectedFolder: PropTypes.object,
    onSelectFolder: PropTypes.func.isRequired,
    onRetrieveFolderError: PropTypes.func,
    portalId: PropTypes.number,
    noFolderSelectedValue: PropTypes.string.isRequired,
    searchFolderPlaceHolder: PropTypes.string.isRequired
};

FolderPicker.defaultProps = {
    portalId: -1
};

