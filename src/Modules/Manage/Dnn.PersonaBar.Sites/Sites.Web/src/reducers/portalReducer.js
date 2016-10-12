import {portal as ActionTypes}  from "../constants/actionTypes";
import {addPortalToList, addTemplateToList} from "../helpers/portal";
export default function portal(state = {
    portals: [],
    templates: [],
    totalCount: 0,
    totalTemplateCount: 0,
    portalTabs: []
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_PORTALS:
            return { ...state,
                portals: action.payload.portals,
                totalCount: action.payload.totalCount
            };
        case ActionTypes.RETRIEVED_PORTAL_TEMPLATES:
            return { ...state,
                templates: action.payload.templates,
                totalTemplateCount: action.payload.totalCount
            };
        case ActionTypes.RETRIEVED_PORTAL_TABS:
            return { ...state,
                portalTabs: action.payload.portalTabs
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
                    let portals = Object.assign([], JSON.parse(JSON.stringify(state.portals)));
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
                let templates = Object.assign([], JSON.parse(JSON.stringify(state.templates)));
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
            return { ...state
            };
    }
}
