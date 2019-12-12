import {vocabularyTermList as ActionTypes}  from "../constants/actionTypes";
import VocabularyTermService from "../services/vocabularyTermService";
import utils from "../utils";

const vocabularyTermActions = {
    getVocabularyTerms(vocabularyId) {
        return (dispatch) => {
            VocabularyTermService.getVocabularyTerms(vocabularyId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_VOCABULARY_TERMS,
                    data: {
                        vocabularyTerms: data.Results,
                        totalCount: data.TotalResults
                    }
                });
            }, (failureMessage) => {
                const errorMessage = JSON.parse(failureMessage.responseText);
                utils.utilities.notifyError(errorMessage.Message);
            });
        };
    },
    addVocabularyTerm(term, totalCount, callback) {
        return (dispatch) => {
            VocabularyTermService.addVocabularyTerm(term, data => {
                term.TermId = data.TermId;
                dispatch({
                    type: ActionTypes.ADDED_VOCABULARY_TERM,
                    payload: {
                        addedTerm: term,
                        totalCount
                    }
                });
                if (callback) {
                    callback();
                }
            }, (failureMessage) => {
                const errorMessage = JSON.parse(failureMessage.responseText);
                utils.utilities.notifyError(errorMessage.Message);
            });
        };
    },
    updateVocabularyTerm(payload, index, callback) {
        return (dispatch) => {
            VocabularyTermService.updateVocabularyTerm(payload, () => {
                dispatch({
                    type: ActionTypes.UPDATED_VOCABULARY_TERM,
                    payload: {
                        updatedTerm: payload,
                        index
                    }
                });
                if (callback) {
                    callback();
                }
            }, (failureMessage) => {
                const errorMessage = JSON.parse(failureMessage.responseText);
                utils.utilities.notifyError(errorMessage.Message);
            });
        };
    },
    deleteVocabularyTerm(termId, vocabularyTerms, totalCount) {
        return (dispatch) => {
            VocabularyTermService.deleteVocabularyTerm(termId, () => {
                dispatch({
                    type: ActionTypes.DELETED_VOCABULARY_TERM,
                    payload: {
                        vocabularyTerms,
                        totalCount
                    }
                });
            }, (failureMessage) => {
                const errorMessage = JSON.parse(failureMessage.responseText);
                utils.utilities.notifyError(errorMessage.Message);
            });
        };
    },
    setTermSelected(updatedTerm, index) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SET_TERM_SELECTED,
                payload: {
                    updatedTerm,
                    index
                }
            });
        };
    },
    clearSelected(vocabularyTerms) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CLEAR_TERMS_SELECTED,
                payload: {
                    vocabularyTerms
                }
            });
        };
    }
};

export default vocabularyTermActions;
