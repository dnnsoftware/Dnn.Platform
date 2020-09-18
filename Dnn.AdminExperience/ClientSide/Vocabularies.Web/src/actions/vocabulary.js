import { vocabulary as ActionTypes } from "../constants/actionTypes";
import VocabularyService from "../services/vocabularyService";
import utils from "../utils";

const vocabularyActions = {
    getVocabularyList(searchParameters, loadMore, callback) {
        return (dispatch) => {
            VocabularyService.getVocabularyList(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_VOCABULARY_LIST,
                    loadMore: loadMore,
                    data: {
                        vocabularyList: data.Results,
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, (failureMessage) => {
                const errorMessage = JSON.parse(failureMessage.responseText);
                utils.utilities.notifyError(errorMessage.Message);
            });
        };
    },
    addVocabulary(vocabulary, totalCount, callback) {
        return (dispatch) => {
            VocabularyService.addVocabulary(vocabulary, data => {
                vocabulary.VocabularyId = data.VocabularyId;
                vocabulary.Type = (vocabulary.ScopeTypeId === 2 ? "Hierarchy" : "Simple");
                dispatch({
                    type: ActionTypes.ADDED_VOCABULARY,
                    payload: {
                        addedVocabulary: vocabulary,
                        totalCount: totalCount
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
    updateVocabulary(payload, index) {
        return (dispatch) => {
            VocabularyService.updateVocabulary(payload, () => {
                dispatch({
                    type: ActionTypes.UPDATED_VOCABULARY,
                    data: {
                        updatedTerm: payload,
                        index: index
                    }
                });
            }, (failureMessage) => {
                const errorMessage = JSON.parse(failureMessage.responseText);
                utils.utilities.notifyError(errorMessage.Message);
            });
        };
    },
    deleteVocabulary(vocabularyId, index, totalCount, callback) {
        return (dispatch) => {
            VocabularyService.deleteVocabulary(vocabularyId, () => {
                dispatch({
                    type: ActionTypes.DELETED_VOCABULARY,
                    payload: {
                        index,
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
    }
};

export default vocabularyActions;
