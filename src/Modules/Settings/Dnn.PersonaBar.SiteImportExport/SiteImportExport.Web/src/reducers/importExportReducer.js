import { importExport as ActionTypes }  from "../constants/actionTypes";

export default function importExport(state = {
    jobs: [],
    portals: []
}, action) {
    switch (action.type) {  
        case ActionTypes.RETRIEVED_PORTALS:
            return { ...state,
                portals: action.portals
            };
        case ActionTypes.RETRIEVED_JOBS:
            return { ...state,
                jobs: action.jobs
            };
        default:
            return { ...state
            };
    }
}
