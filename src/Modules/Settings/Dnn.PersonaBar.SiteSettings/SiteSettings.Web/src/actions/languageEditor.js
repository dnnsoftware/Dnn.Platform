import {languageEditor as ActionTypes}  from "constants/actionTypes";
const languageEditorActions = {
    setLanguageBeingEdited(language, callback) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SET_LANGUAGE_BEING_EDITED,
                payload: language
            });
        };
    }
};

export default languageEditorActions;
