import { importExport as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";

const jobsActions = {
    siteSelected(portalId, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SELECTED_SITE,
                portalId: portalId
            });
            if (callback) {
                callback();
            }
        };
    },

    navigateWizard(wizardStep, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.GO_TO_WIZARD_STEP,
                payload: {
                    wizardStep
                }
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },

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

    getPortalLogo(portalId, callback, errorCallback) {
        return (dispatch) => {
            ApplicationService.getPortalLogo(portalId, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PORTAL_LOGO,
                    logoUrl: data.LogoUrl
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
                    jobs: data.Jobs,
                    totalJobs: data.TotalJobs,
                    portalName: data.PortalName
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },

    getJobDetails(jobId, callback, errorCallback) {
        return (dispatch) => {
            ApplicationService.getJobDetails(jobId, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_JOB_DETAILS,
                    job: data
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },

    exportSite(payload, callback, errorCallback) {
        return (dispatch) => {
            ApplicationService.exportSite(payload, data => {
                dispatch({
                    type: ActionTypes.SUBMITTED_EXPORT_REQUEST,
                    jobId: data.jobId
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (errorCallback) {
                    errorCallback(data);
                }
            });
        };
    }
};

export default jobsActions;
