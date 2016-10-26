import {portal as ActionTypes}  from "constants/actionTypes";
import {addPortalToList, addTemplateToList} from "helpers/portal";
import utilities from "utils";
export default function portal(state = {
    portals: [],
    templates: [],
    totalCount: 0,
    totalTemplateCount: 0,
    portalBeingExported: {}
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_PORTALS:
            return { ...state,
                portals: action.payload.portals,
                totalCount: action.payload.totalCount
            };
        case ActionTypes.SET_PORTAL_BEING_EXPORTED:
            return { ...state,
                portalBeingExported: action.payload
            };
        case ActionTypes.RETRIEVED_PORTAL_TEMPLATES:
            return { ...state,
                templates: action.payload.templates,
                totalTemplateCount: action.payload.totalCount
            };
        case ActionTypes.DELETED_PORTAL_TEMPLATE:
            return { ...state,
                portals: [
                    ...state.portals.slice(0, action.payload.index),
                    ...state.portals.slice(action.payload.index + 1)
                ]
            };
        case ActionTypes.CREATED_PORTAL_TEMPLATE:
            {
                if (action.payload.Success) {
                    let portals = Object.assign([], utilities.getObjectCopy(state.portals));
                    let totalCount = Object.assign(state.totalCount);
                    return { ...state,
                        portals: addPortalToList(portals, action.payload.Portal),
                        totalCount: totalCount + 1
                    };
                }
                return { ...state
                };
            }
        case ActionTypes.EXPORTED_PORTAL_TEMPLATE: {
            if (action.payload.Success) {
                let templates = Object.assign([], utilities.getObjectCopy(state.templates));
                let totalTemplateCount = Object.assign(state.totalTemplateCount);
                return { ...state,
                    templates: addTemplateToList(templates, action.payload.Template),
                    totalTemplateCount: totalTemplateCount + 1
                };
            }
            return { ...state
            };
        }
        default:
            return state;
    }
}
