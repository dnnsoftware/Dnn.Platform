import {
    portal as ActionTypes
} from "actionTypes";
import {
    addTemplateToList,
    getFinalSwitchCase
} from "./helpers";
import utilities from "utils";

const switchCase = [{
    condition: ActionTypes.SET_PORTAL_BEING_EXPORTED,
    functionToRun: (state, action) => {
        return {
            portalBeingExported: action.payload
        };
    }
}, {
    condition: ActionTypes.RETRIEVED_PORTAL_TEMPLATES,
    functionToRun: (state, action) => {
        return {
            templates: action.payload.templates,
            totalTemplateCount: action.payload.totalCount
        };
    }
}, {
    condition: ActionTypes.EXPORTED_PORTAL_TEMPLATE,
    functionToRun: (state, action) => {
        let templates = Object.assign([], utilities.getObjectCopy(
            state.templates));
        let totalTemplateCount = Object.assign(state.totalTemplateCount);
        return {
            templates: addTemplateToList(templates, action.payload
                .Template),
            totalTemplateCount: totalTemplateCount + 1
        };
    }
}];

export default function getReducer(initialState, additionalCases) {
    return function common(state = Object.assign({
        templates: [],
        totalTemplateCount: 0,
        portalBeingExported: {}
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
