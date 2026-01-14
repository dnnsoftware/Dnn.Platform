import { serversTab as ActionTypes } from "../constants/actionTypes";
import serversTabService from "../services/serversTabService";
import localization from "../localization";

const serversTabActions = {
    loadServers() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_SERVERS,
            });

            serversTabService
                .getServers()
                .then((response) => {
                    dispatch({
                        type: ActionTypes.LOADED_SERVERS,
                        payload: {
                            servers: response,
                        },
                    });
                })
                .catch(() => {
                    dispatch({
                        type: ActionTypes.ERROR_LOADING_SERVERS,
                        payload: {
                            errorMessage: localization.get("errorMessageLoadingServers"),
                        },
                    });
                });
        };
    },

    deleteServer(serverId) {
        return (dispatch) => {
            serversTabService
                .deleteServer(serverId)
                .then(() => {
                    serversTabActions.loadServers()(dispatch);
                })
                .catch((err) => {
                    // eslint-disable-next-line no-console
                    console.error(err);
                    dispatch({
                        type: ActionTypes.ERROR_DELETING_SERVER,
                        payload: {
                            errorMessage: localization.get("errorMessageDeleteServers"),
                        },
                    });
                });
        };
    },

    deleteNonActiveServers() {
        return (dispatch) => {
            serversTabService
                .deleteNonActiveServers()
                .then(() => {
                    serversTabActions.loadServers()(dispatch);
                })
                .catch(() => {
                    dispatch({
                        type: ActionTypes.ERROR_DELETING_SERVER,
                        payload: {
                            errorMessage: localization.get("errorMessageDeleteServers"),
                        },
                    });
                });
        };
    },
};

export default serversTabActions;
