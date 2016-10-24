import { createPackage as ActionTypes } from "constants/actionTypes";
import undoable from "redux-undo";

const newPackagePayload = {
    useExistingManifest: false,
    selectedManifest: "",
    selectedManifestKey: "",
    archiveName: "",
    manifestName: "",
    createManifest: true,
    createPackage: true,
    reviewManifest: true
};

const createPackage = function (state = {
    packageManifest: {},
    createdManifest: {},
    createdPackage: {},
    packagePayload: newPackagePayload,
    currentStep: 0
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_PACKAGE_MANIFEST:
            return { ...state,
                packageManifest: action.payload,
                packagePayload: newPackagePayload
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
        case ActionTypes.UPDATED_PACKAGE_MANIFEST:
            return { ...state,
                packageManifest: action.payload
            };
        case ActionTypes.UPDATED_PACKAGE_PAYLOAD:
            return { ...state,
                packagePayload: action.payload
            };
        default:
            return { ...state
            };
    }
};

const undoableCreatePackage = undoable(createPackage);
export default undoableCreatePackage;
