import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import extension from "./extensionReducer";
import installation from "./installationReducer";
import folder from "./folderReducer";
import moduleDefinition from "./moduleDefinitionReducer";
import createPackage from "./createPackageReducer";
const rootReducer = combineReducers({
    pagination,
    visiblePanel,
    extension,
    installation,
    folder,
    moduleDefinition,
    createPackage
});

export default rootReducer;
