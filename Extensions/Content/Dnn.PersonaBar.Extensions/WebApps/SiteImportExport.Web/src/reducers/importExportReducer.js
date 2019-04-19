import { importExport as ActionTypes } from "../constants/actionTypes";

export default function importExport(state = {
    jobs: [],
    portalId: -1,
    portals: [],
    totalJobs: 0,
    portalName: null,
    logoUrl: null,
    exportWizardStep: 0,
    importWizardStep: 0,
    exportJobId: -1,
    importPackages: [],
    selectedPackage: undefined,
    verificationSummary: undefined
}, action) {
    if (!action) {
        return { ...state };
    }
    switch (action.type) {
        case ActionTypes.SELECTED_SITE:
            return {
                ...state,
                portalId: action.portalId,
                portalName: action.portalName,
                currentJobId: action.currentJobId
            };
        case ActionTypes.SELECTED_JOB:
            return {
                ...state,
                currentJobId: action.currentJobId
            };
        case ActionTypes.GO_TO_WIZARD_STEP:
            return {
                ...state,
                exportWizardStep: action.payload.wizardStep
            };
        case ActionTypes.RETRIEVED_PORTALS:
            return {
                ...state,
                portals: action.portals
            };
        case ActionTypes.RETRIEVED_JOBS:
            return {
                ...state,
                jobs: action.jobs,
                totalJobs: action.totalJobs,
                lastExportTime: action.lastExportTime,
                lastImportTime: action.lastImportTime
            };
        case ActionTypes.RETRIEVED_JOB_DETAILS:
            return {
                ...state,
                job: action.job
            };
        case ActionTypes.SUBMITTED_EXPORT_REQUEST:
            return {
                ...state,
                exportJobId: action.jobId
            };
        case ActionTypes.SUBMITTED_IMPORT_REQUEST:
            return {
                ...state,
                importJobId: action.jobId,
                packageVerified: action.packageVerified
            };
        case ActionTypes.RETRIEVED_IMPORT_PACKAGES:
            return {
                ...state,
                importPackages: action.importPackages,
                totalPackages: action.total,
                selectedPackage: action.selectedPackage,
                importWizardStep: 0,
                packageVerified: false
            };
        case ActionTypes.GO_TO_IMPORT_WIZARD_STEP:
            return {
                ...state,
                importWizardStep: action.importWizardStep
            };
        case ActionTypes.VERIFIED_IMPORT_PACKAGE:
            return {
                ...state,
                importSummary: action.importSummary
            };
        case ActionTypes.SELECTED_PACKAGE:
            return {
                ...state,
                selectedPackage: action.selectedPackage,
                importSummary: action.importSummary
            };
        case ActionTypes.VERIFIED_PACKAGE:
            return {
                ...state,
                packageVerified: action.packageVerified
            };
        case ActionTypes.RETRIEVED_LAST_EXPORT_DATE:
            return {
                ...state,
                lastExportTime: action.lastExportTime
            };
        default:
            return {
                ...state
            };
    }
}
