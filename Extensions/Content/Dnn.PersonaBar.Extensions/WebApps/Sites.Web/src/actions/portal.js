import {
    portal as ActionTypes
} from "dnn-sites-common-action-types";
import {
    PortalService
} from "services";
import utilities from "utils";

function errorCallback(message) {
    utilities.notifyError(JSON.parse(message.responseText).Message, 5000);
}
const portalActions = {
    loadPortals(searchParameters, callback) {
        return (dispatch) => {
            PortalService.getPortals(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PORTALS,
                    payload: {
                        portals: data.Results,
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    }
};

export default portalActions;
