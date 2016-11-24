import { createPackage as ActionTypes } from "constants/actionTypes";

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

export default function createPackage(state = {
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
                createdPackage: action.payload.Logs
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
        case ActionTypes.RETRIEVED_GENERATED_MANIFEST:
            return { ...state,
                packagePayload: Object.assign(JSON.parse(JSON.stringify(state.packagePayload)), { selectedManifest: action.payload.Content })
            };
        case ActionTypes.REFRESH_PACKAGE_FILES:
            return { ...state,
                packageManifest: Object.assign(JSON.parse(JSON.stringify(state.packageManifest)), { files: action.payload })
            };
        default:
            return state;
    }
}
