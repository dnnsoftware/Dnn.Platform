import React, {Component, PropTypes} from "react";
import ReactDOM from "react-dom";
import Folders from "./Folders";
import { Scrollbars } from "react-custom-scrollbars";
import "./style.less";

const searchIconImage = require("!raw!./img/search.svg");

export default class FolderSelector extends Component {

    constructor() {
        super();
        this.state = {
            showFolderPicker: false,
            searchFolderText: ""
        };
        this.timeOut = null;
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
        const node = ReactDOM.findDOMNode(this);
        if (node && node.contains(e.target) || e.target.className === "clear-button") {
            return;
        }
        this.hide();
    }

    hide() {
        this.setState({ showFolderPicker: false});
    }

    onFolderChange(folder) {
        this.hide();
        this.props.onFolderChange(folder);
    }

    onChangeSearchFolderText(e) {
        const searchFolderText = e.target.value ? e.target.value : "";
        this.setState({ searchFolderText });
        clearTimeout(this.timeOut);
        this.timeOut = setTimeout(() => {this.props.searchFolder(searchFolderText.toLowerCase());}, 500);
    }

    clearSearch(e) {
        e.preventDefault();
        this.setState({ searchFolderText: "" });
        this.props.searchFolder();
    }   

    onFoldersClick() {
        const {showFolderPicker} = this.state;
        this.setState({ showFolderPicker: !showFolderPicker });
    }

    getSearchIcon() {
        /* eslint-disable react/no-danger */
        return (<div className="search-icon" dangerouslySetInnerHTML={{ __html: searchIconImage }} />);
        /* eslint-enable react/no-danger */
    }

    render() {
        const {selectedFolder, folders, onParentExpands, noFolderSelectedValue, searchFolderPlaceHolder} = this.props;
        const selectedFolderText = selectedFolder ? selectedFolder.value : "<" + noFolderSelectedValue + ">";
        const searchIcon = this.getSearchIcon();

        return ( 
            <div className="dnn-folder-selector">
                <div className="selected-item" onClick={this.onFoldersClick.bind(this) }>
                    {selectedFolderText}
                </div>
                <div className={"folder-selector-container" + (this.state.showFolderPicker ? " show" : "") } >
                    <div className="inner-box">
                        <div className="search">
                            <input 
                                type="text" 
                                value={this.state.searchFolderText} 
                                onChange={this.onChangeSearchFolderText.bind(this) } 
                                placeholder={searchFolderPlaceHolder}
                                aria-label="Search" />
                            {this.state.searchFolderText && 
                                <div onClick={this.clearSearch.bind(this)} className="clear-button">×</div>
                            }
                            {searchIcon}
                        </div>
                        <div className="items">
                            <Scrollbars className="scrollArea content-vertical"
                                autoHeight
                                autoHeightMin={0}
                                autoHeightMax={200}>
                                <Folders
                                    folders={folders}
                                    onParentExpands={onParentExpands}
                                    onFolderChange={this.onFolderChange.bind(this) }/>
                            </Scrollbars>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}


FolderSelector.propTypes = {
    folders: PropTypes.object.isRequired,
    onFolderChange: PropTypes.func.isRequired,
    onParentExpands: PropTypes.func.isRequired,
    selectedFolder: PropTypes.object,
    searchFolder: PropTypes.func.isRequired,
    noFolderSelectedValue: PropTypes.string.isRequired,
    searchFolderPlaceHolder: PropTypes.string.isRequired
};