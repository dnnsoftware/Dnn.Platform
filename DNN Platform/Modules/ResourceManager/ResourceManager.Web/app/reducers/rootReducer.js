import { combineReducers } from "redux";
import module from "./module";
import folderPanel from "./folderPanel";
import breadcrumbs from "./breadcrumbs";
import addFolderPanel from "./addFolderPanel";
import addAssetPanel from "./addAssetPanel";
import dialogModal from "./dialogModal";
import infiniteScroll from "./infiniteScroll";
import messageModal from "./messageModal";
import itemDetails from "./itemDetails";
import topBar from "./topBar";
import manageFolderTypesPanel from "./manageFolderTypesPanel";

const rootReducer = combineReducers({
    module,
    folderPanel,
    breadcrumbs,
    addFolderPanel,
    addAssetPanel,
    dialogModal,
    infiniteScroll,
    messageModal,
    itemDetails,
    topBar,
    manageFolderTypesPanel,
});

export default rootReducer;