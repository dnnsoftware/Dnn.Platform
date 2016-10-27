import {
    visiblePanel as ActionTypes
} from "actionTypes";

const switchCase = [{
    condition: ActionTypes.SELECT_PANEL,
    functionToRun: (state, action) => {
        return {
            selectedPage: action.payload.selectedPage
            };
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
        selectedPage: 0
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
