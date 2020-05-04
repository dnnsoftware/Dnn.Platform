import React, { PropTypes} from "react";
import FolderSelector from "./FolderSelector";
import internalService from "../../services/internalService";

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

export default class FolderPicker extends React.Component {

    constructor(props) {
        super(props);

        this.state = {
            folders: null
        };
    }

    componentDidMount() {
        this.getFolders();
    }

    componentWillUnmount() {   
        this.unmounted = true;
    }

    getFolders(searchText) {
        let folderPromise = searchText ? internalService.searchFolders(searchText) : internalService.getFolders();
        folderPromise.then(
            this.setFolders.bind(this)
        );
    }

    getChildrenFolders(parentId) {
        internalService.getFolderDescendant(parentId)
        .then(
            this.addChildFolders.bind(this, parentId)
        );
    }

    setFolders(result) {
        if (this.unmounted) {
            return;
        }
        
        const { props } = this;
        let homeFolderId = props.homeFolderId;

        let source = result.Tree;
        let filtered = this.findNode(source, (n) => {return n !== null && n.data.key === homeFolderId.toString();});
        source.children.splice(0, source.children.length, filtered);

        this.setState({ folders: result.Tree });
    }

    findNode(node, condition) {
        if (condition(node)) {
            return node;
        }

        for (let i = 0; i < node.children.length; i++) {
            let found = this.findNode(node.children[i], condition);
            if (found !== null) {
                return found;
            }
        }

        return null;
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
        const {changeFolder} = this.props;

        return (
            <FolderSelector
                folders={this.state.folders}
                searchFolder={this.getFolders.bind(this)}
                onParentExpands={this.getChildrenFolders.bind(this)}
                onFolderChange={changeFolder}
                {...this.props} />
        );    
    }
}

/**
 * propTypes
 * @property {object} selectedFolder selected folder. Should be null or have a key and value properties
 * @property {object} onSelectFolder called when change folder. Pass as parameter a selectedFolder object
 * @property {object} onRetrieveFolderError callback for error when retrieve data
 */

FolderPicker.propTypes = {
    selectedFolder: PropTypes.object,
    changeFolder: PropTypes.func,
    onRetrieveFolderError: PropTypes.func,
    noFolderSelectedValue: PropTypes.string.isRequired,
    searchFolderPlaceHolder: PropTypes.string.isRequired,
    homeFolderId: PropTypes.number
};
