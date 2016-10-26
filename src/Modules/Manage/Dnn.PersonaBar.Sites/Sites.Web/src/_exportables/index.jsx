import SitesListView from "./src/ListView";
import ExportPortal from "./src/ExportPortal";
import PortalListReducer from "./src/reducers/portalListReducer";

if(!window.dnn){
    window.dnn = {};
}
if(!window.dnn.Sites){
    window.dnn.Sites = {};
}

window.dnn.Sites.SitesListView = SitesListView;
window.dnn.Sites.ExportPortal = ExportPortal;
window.dnn.Sites.PortalListReducer = PortalListReducer;