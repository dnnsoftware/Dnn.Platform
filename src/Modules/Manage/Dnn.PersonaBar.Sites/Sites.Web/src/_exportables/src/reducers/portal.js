import {
    portal as ActionTypes
} from "actionTypes";
import {
    addPortalToList,
    addTemplateToList
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
},  {
    condition: ActionTypes.DELETED_PORTAL,
    functionToRun: (state, action) => {
        return {
            portals: [
                ...state.portals.slice(0, action.payload.index),
                ...state.portals.slice(action.payload.index + 1)
            ]
        };
    }
}, {
    condition: ActionTypes.CREATED_PORTAL,
    functionToRun: (state, action) => {
        if (action.payload.Success) {
            let portals = Object.assign([], utilities.getObjectCopy(
                state.portals));
            let totalCount = Object.assign(state.totalCount);
            return {
                portals: addPortalToList(portals, action.payload.Portal),
                totalCount: totalCount + 1
            };
        }
        return state;
    }
}];

function getFinalSwitchCase(switchCase, additionalCases) {
    let _switchCase = switchCase;
    if (Object.prototype.toString.call(additionalCases) === "[object Array]") {
        additionalCases.forEach((extraCase) => {
            let alreadyExists = false;
            let indexToChange = 0;
            _switchCase.forEach((item, index) => {
                if (extraCase.condition === item.condition) {
                    alreadyExists = true;
                    indexToChange = index;
                }
            });
            if (!alreadyExists) {
                _switchCase.push(extraCase);
            } else {
                _switchCase[indexToChange] = extraCase;
            }
        });
    }
    return _switchCase;
}
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
