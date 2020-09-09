import React from "react";
import PropTypes from "prop-types";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import folderPanelActions from "../actions/folderPanelActions";
import topBarActions from "../actions/topBarActions";
import localizeService from "../services/localizeService";
import DropDown from "../../../../../../Dnn.AdminExperience/ClientSide/Dnn.React.Common/src/Dropdown";
import FolderPicker from "../components/FolderSelector/FolderPicker";
import debounce from "lodash/debounce";

const refreshSyncIcon = require("!raw-loader!../../../Images/redeploy.svg").default;

class TopBarContainer extends React.Component {
    componentWillMount() {
        this.debouncedSearch = debounce(this.searchHandler.bind(this, true), 500);
    }

    onChangeFolder(folder) {
        const {folderPanelState, loadContent} = this.props;
        const folderId = parseInt(folder.key);
        loadContent(folderPanelState, folderId);
    }

    searchHandler(checkLength) {
        const {folderPanelState, changeSearchingValue, searchFiles, loadContent} = this.props;
        const {currentFolderId} = folderPanelState;
        const oldSearch = folderPanelState.search;
        let search = this.props.search;

        search = checkLength && search.length < 3 ? "" : search;

        if (!oldSearch && !search) {
            return;
        }

        changeSearchingValue(search);
        if (!search) {
            loadContent(folderPanelState, currentFolderId);
        }
        else {
            searchFiles(folderPanelState, search);
        }
    }

    onSearchChanged(event) {
        this.props.changeSearchField(event.target.value);
        this.debouncedSearch();
    }

    onSortingChange(option) {
        const {changeSorting, loadContent, folderPanelState} = this.props;
        const {currentFolderId} = folderPanelState;
        const newFolderPanelState = { ...folderPanelState, sorting: option.value };

        changeSorting(option.value);
        loadContent(newFolderPanelState, currentFolderId);
    }

    onRefreshSyncChange(folderPanelState, currentFolderId, option) {
        switch (option.value) {
            case "Refresh":
                this.props.loadContent(folderPanelState, currentFolderId);
                break;
        
            case "SyncThisFolder":
                this.props.syncContent(folderPanelState, false);
                break;
        
            case "SyncThisFolderAndSubfolders":
                this.props.syncContent(folderPanelState, true);
                break;
    
            default:
                break;
        }
    }

    render() {
        const {folderPanelState, search, userLogged, homeFolderId} = this.props;
        const {currentFolderId, currentFolderName, sortOptions, sorting} = folderPanelState;

        return (
            <div className="header-container">
                <div className="folder-picker-container three-columns">
                    {userLogged ?
                        <div className="rm-folder-picker-refresh-sync-container">
                            <DropDown 
                                className="rm-refresh-sync-dropdown" 
                                options={[
                                    { value: "Refresh", label: localizeService.getString("Refresh") },
                                    { value: "SyncThisFolder", label: localizeService.getString("SyncThisFolder") },
                                    { value: "SyncThisFolderAndSubfolders", label: localizeService.getString("SyncThisFolderAndSubfolders") }
                                ]} 
                                onSelect={option => this.onRefreshSyncChange(folderPanelState, currentFolderId, option)} 
                                withBorder={false} 
                                fixedHeight={200}
                                label=""
                                prependWith={<span dangerouslySetInnerHTML={{__html: refreshSyncIcon}}></span>}
                                withIcon={false} />
                            <FolderPicker selectedFolder={{key: currentFolderId, value: currentFolderName}}
                                    changeFolder={this.onChangeFolder.bind(this)} 
                                    homeFolderId={homeFolderId}
                                    noFolderSelectedValue={localizeService.getString("NoFolderSelected")} 
                                    searchFolderPlaceHolder={localizeService.getString("SearchFolder")}/>
                        </div> 
                        : <span>{localizeService.getString("DisabledForAnonymousUsersMessage")}</span>
                    }
                </div>
                <div className="sort-container">
                    <DropDown className="rm-dropdown" options={sortOptions} value={sorting} onSelect={this.onSortingChange.bind(this)} 
                        withBorder={false} fixedHeight={200}/>
                </div>
                <div className="search-box-container">
                    <input className="assets-input" type="search" placeholder={localizeService.getString("SearchInputPlaceholder")}
                        onChange={this.onSearchChanged.bind(this)} value={search} >
                    </input>
                    <a className="icon-button search-button" onClick={this.searchHandler.bind(this, false)}></a>
                </div>
            </div>
        );
    }
}

TopBarContainer.propTypes = {
    folderPanelState: PropTypes.object,
    loadContent: PropTypes.func,
    syncContent: PropTypes.func,
    changeSearchingValue: PropTypes.func,
    searchFiles: PropTypes.func,
    changeSorting: PropTypes.func,
    changeSearchField: PropTypes.func,
    search: PropTypes.string,
    userLogged: PropTypes.bool,
    homeFolderId: PropTypes.number
};

function mapStateToProps(state) {
    const moduleState = state.module;
    const folderPanelState = state.folderPanel;
    const topBarState = state.topBar;

    return {
        folderPanelState,
        search: topBarState.search || "",
        userLogged: moduleState.userLogged,
        homeFolderId: moduleState.homeFolderId
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators({
            loadContent: folderPanelActions.loadContent,
            syncContent: folderPanelActions.syncContent,
            searchFiles: folderPanelActions.searchFiles,
            changeSearchingValue: folderPanelActions.changeSearchingValue,
            changeSorting: folderPanelActions.changeSorting,
            changeSearchField: topBarActions.changeSearchField
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(TopBarContainer);