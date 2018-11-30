import {
    portal as ActionTypes
} from "actionTypes";
import {
    addPortalToList,
    getFinalSwitchCase
} from "./helpers";
import utilities from "utils";

const switchCase = [{
    condition: ActionTypes.RETRIEVED_PORTALS,
    functionToRun: (state, action) => {
        return {
            portals: action.payload.portals,
            totalCount: action.payload.totalCount
        };
    }
}, {
    condition: ActionTypes.DELETED_PORTAL,
    functionToRun: (state, action) => {
        let totalCount = Object.assign(state.totalCount);
        return {
            portals: [
                ...state.portals.slice(0, action.payload.index),
                ...state.portals.slice(action.payload.index + 1)
            ],
            totalCount: totalCount - 1
        };
    }
}, {
    condition: ActionTypes.RETRIEVED_PORTALS_CONCAT,
    functionToRun: (state, action) => {
        return {
            portals: state.portals.concat(action.payload.portals),
            totalCount: action.payload.totalCount
        };
    }
}, {
    condition: ActionTypes.CREATED_PORTAL,
    functionToRun: (state, action) => {
        let portals = Object.assign([], utilities.getObjectCopy(
            state.portals));
        let totalCount = Object.assign(state.totalCount);
        return {
            portals: addPortalToList(portals, action.payload.Portal),
            totalCount: totalCount + 1
        };
    }
}, {
    condition: ActionTypes.UPDATE_PORTAL_PARAMETER,
    functionToRun: (state, action) => {
        let portals = Object.assign([], utilities.getObjectCopy(state.portals));
        let totalCount = Object.assign(state.totalCount);
        let portal = portals.find(p => p.PortalID === action.payload.portalId);
        if (portal) {
            portal[action.payload.name] = action.payload.value;
        }

        return {
            portals: portals,
            totalCount: totalCount
        };
    }
}];

export default function getReducer(initialState, additionalCases) {
    return function common(state = Object.assign({
        portals: [],
        totalCount: 0
    }, initialState), action) {
        let _switchCase = getFinalSwitchCase(switchCase, additionalCases);

        let returnCase = { ...state };

        _switchCase.forEach((to) => {
            if (to.condition === action.type) {
                const stuffToAdd = to.functionToRun(state, action);
                returnCase = Object.assign(returnCase, stuffToAdd);
            }
        });

        return returnCase;
    };
}
