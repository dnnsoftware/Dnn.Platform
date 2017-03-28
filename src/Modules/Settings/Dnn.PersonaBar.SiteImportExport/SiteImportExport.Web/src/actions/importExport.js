import { importExport as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";

const jobsActions = {
    siteSelected(portalId, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SELECTED_SITE,
                portalId: portalId === undefined ? -1 : portalId
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

    getPortalLocales(portalId, callback, errorCallback) {
        return (dispatch) => {
            ApplicationService.getPortalLocales(portalId, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PORTAL_LOCALES
                });
                if (callback) {
                    callback(data);
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

    selectPackage(packageId, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SELECTED_PACKAGE,
                selectedPackageId: packageId
            });
            if (callback) {
                callback();
            }
        };
    },

    getImportPackages(callback, errorCallback) {
        return (dispatch) => {
            ApplicationService.getImportPackages((data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_IMPORT_PACKAGES,
                    importPackages: data
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);

            /*dispatch({
                type: ActionTypes.RETRIEVED_IMPORT_PACKAGES,
                importPackages: [
                    { file: "12-25-2016_export.resource", name: "Default Website - English (United States)", description: "We provide a suite of solutions for creating rich, rewarding online experiences for customers, partners and employees. Our products and technology are the foundation for 750,000+ websites worldwide. In addition to our commercial CMS, Evoq, we're the steward of the DotNetNuke Open Source Project.", image: "https://cdn.colorlib.com/wp/wp-content/uploads/sites/2/pages-fullscreen-html5-portfolio-template.jpg" },
                    { file: "12-25-2016_export.resource", name: "New Application Marketing Template", description: "We provide a suite of solutions for creating rich, rewarding online experiences for customers, partners and employees. Our products and technology are the foundation for 750,000+ websites worldwide. In addition to our commercial CMS, Evoq, we're the steward of the DotNetNuke Open Source Project.", image: "https://cdn.colorlib.com/wp/wp-content/uploads/sites/2/canvas-creative-one-page-business-template.jpg" },
                    { file: "12-25-2016_export.resource", name: "Product Launch Site Template", description: "We provide a suite of solutions for creating rich, rewarding online experiences for customers, partners and employees. Our products and technology are the foundation for 750,000+ websites worldwide. In addition to our commercial CMS, Evoq, we're the steward of the DotNetNuke Open Source Project.", image: "https://cdn.colorlib.com/wp/wp-content/uploads/sites/2/rhythm-modern-multipurpose-business-html5-template.jpg" },
                    { file: "12-25-2016_export.resource", name: "Default Website - English (United States)", description: "We provide a suite of solutions for creating rich, rewarding online experiences for customers, partners and employees. Our products and technology are the foundation for 750,000+ websites worldwide. In addition to our commercial CMS, Evoq, we're the steward of the DotNetNuke Open Source Project.", image: "https://cdn.colorlib.com/wp/wp-content/uploads/sites/2/zap-one-page-parallax-business-html-template.jpg" },
                    { file: "12-25-2016_export.resource", name: "New Application Marketing Template", description: "We provide a suite of solutions for creating rich, rewarding online experiences for customers, partners and employees. Our products and technology are the foundation for 750,000+ websites worldwide. In addition to our commercial CMS, Evoq, we're the steward of the DotNetNuke Open Source Project.", image: "https://cdn.colorlib.com/wp/wp-content/uploads/sites/2/carna-corporate-portfolio-site-template.jpg" },
                    { file: "12-25-2016_export.resource", name: "Product Launch Site Template", description: "We provide a suite of solutions for creating rich, rewarding online experiences for customers, partners and employees. Our products and technology are the foundation for 750,000+ websites worldwide. In addition to our commercial CMS, Evoq, we're the steward of the DotNetNuke Open Source Project.", image: "https://cdn.colorlib.com/wp/wp-content/uploads/sites/2/lydia-personal-photography-boostrap-template.jpg" }
                ]
            });
            if (callback) {
                callback();
            }*/
        };
    },

    verifyImportPackage(packageId, callback, errorCallback) {
        return (dispatch) => {
            ApplicationService.verifyImportPackage(packageId, (data) => {
                dispatch({
                    type: ActionTypes.VERIFIED_IMPORT_PACKAGE,
                    isValid: data ? false : true
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    }
};

export default jobsActions;
