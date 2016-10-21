import { createPackage as ActionTypes } from "constants/actionTypes";


export default function createPackage(state = {
    packageManifest: {},
    createdManifest: {},
    createdPackage: {},
    currentStep: 0
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_PACKAGE_MANIFEST:
            return { ...state,
                packageManifest: action.payload
            };
        case ActionTypes.CREATED_PACKAGE_MANIFEST:
            return { ...state,
                createdManifest: action.payload
            };
        case ActionTypes.CREATED_PACKAGE:
            return { ...state,
                createdPackage: action.payload
            };
        case ActionTypes.GO_TO_STEP:
            return { ...state,
                currentStep: action.payload
            };
        default:
            return { ...state
            };
    }
}
