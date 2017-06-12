import { importExport as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";

const jobsActions = {

    getInitialPortalTabs(PortalTabsParameters, callback) {
        ApplicationService.getInitialPortalTabs(PortalTabsParameters, (response) => {
            if (callback) {
                callback(response);
            }
        });
    },

    getDescendantPortalTabs(PortalTabsParameters, ParentTabId, callback) {
        const ParentTabIdObj = {parentId: ParentTabId};
        ApplicationService.getDescendantPortalTabs(PortalTabsParameters, ParentTabIdObj, (response) => {
            if (callback) {
                callback(response);
            }
        });
    },

    siteSelected(portalId, portalName, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SELECTED_SITE,
                portalId: portalId === undefined ? -1 : portalId,
                portalName: portalName,
                currentJobId: null
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
                    callback(data);
                }
            }, errorCallback);
        };
    },

    jobSelected(jobId, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SELECTED_JOB,
                currentJobId: jobId === undefined ? null : jobId
            });
            if (callback) {
                callback();
            }
        };
    },

    getAllJobs(parameters, callback, errorCallback) {
        return (dispatch) => {
            ApplicationService.getAllJobs(parameters, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_JOBS,
                    jobs: data.Jobs,
                    totalJobs: data.TotalJobs,
                    lastExportTime: data.LastExportTimeString,
                    lastImportTime: data.LastImportTimeString
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },

    getLastExportTime(parameters, callback, errorCallback) {
        return (dispatch) => {
            ApplicationService.getLastJobTime(parameters, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_LAST_EXPORT_DATE,
                    lastExportTime: data.lastTime
                });
                if (callback) {
                    callback(data);
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
                    callback(data);
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
    },

    importSite(payload, callback, errorCallback) {
        return (dispatch) => {
            ApplicationService.importSite(payload, data => {
                dispatch({
                    type: ActionTypes.SUBMITTED_IMPORT_REQUEST,
                    jobId: data.jobId,
                    packageVerified: false
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
    },

    importWizardGoToSetp(step, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.GO_TO_IMPORT_WIZARD_STEP,
                importWizardStep: step
            });
            if (callback) {
                callback();
            }
        };
    },

    selectPackage(pkg, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SELECTED_PACKAGE,
                selectedPackage: pkg,
                importSummary: undefined
            });
            if (callback) {
                callback();
            }
        };
    },

    packageVerified(verified, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.VERIFIED_PACKAGE,
                packageVerified: verified
            });
            if (callback) {
                callback();
            }
        };
    },

    getImportPackages(parameters, callback, errorCallback) {
        return (dispatch) => {
            ApplicationService.getImportPackages(parameters, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_IMPORT_PACKAGES,
                    importPackages: data.packages,
                    total: data.total,
                    selectedPackage: null,
                    importWizardStep: 0,
                    packageVerified: false
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },

    verifyImportPackage(packageId, callback, errorCallback) {
        return (dispatch) => {
            setTimeout(() => {
                ApplicationService.verifyImportPackage(packageId, (data) => {
                    dispatch({
                        type: ActionTypes.VERIFIED_IMPORT_PACKAGE,
                        importSummary: data
                    });
                    if (callback) {
                        callback();
                    }
                }, errorCallback);
            }, 2000);
        };
    },

    cancelJob(jobId, callback, errorCallback) {
        return (dispatch) => {
            ApplicationService.cancelJob(jobId, data => {
                dispatch({
                    type: ActionTypes.CANCELLED_JOB
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
    },

    deleteJob(jobId, callback, errorCallback) {
        return (dispatch) => {
            ApplicationService.deleteJob(jobId, data => {
                dispatch({
                    type: ActionTypes.REMOVED_JOB
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
