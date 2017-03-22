import { importExport as ActionTypes } from "../constants/actionTypes";

export default function importExport(state = {
    jobs: [],
    portals: [],
    totalJobs: 0,
    portalName: null,
    logoUrl: null
}, action) {
    switch (action.type) {
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
                portalName: action.portalName
            };
        case ActionTypes.RETRIEVED_JOB_DETAILS:
            return {
                ...state,
                job: action.job
            };
        case ActionTypes.RETRIEVED_PORTAL_LOGO:
            return {
                ...state,
                logoUrl: action.logoUrl
            };
        default:
            return {
                ...state
            };
    }
}
