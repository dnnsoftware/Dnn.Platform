import { importExport as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";

const jobsActions = {
    getPortals(callback, errorCallback) {
        return (dispatch) => {
            ApplicationService.getPortals((data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PORTALS,
                    portals: data.Results
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },

    getAllJobs(parameters, callback, errorCallback) {
        return (dispatch) => {
            ApplicationService.getAllJobs(parameters, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_JOBS,
                    jobs: data
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    }
};

export default jobsActions;
