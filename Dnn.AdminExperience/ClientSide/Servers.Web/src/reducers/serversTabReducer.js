import { serversTab as ActionTypes } from "../constants/actionTypes";

export default function serversTabReducer(
  state = {
    servers: [],
    errorMessage: "",
  },
  action,
) {
  switch (action.type) {
    case ActionTypes.LOAD_SERVERS:
      return { ...state, servers: [], errorMessage: "" };
    case ActionTypes.LOADED_SERVERS:
      return { ...state, servers: action.payload.servers, errorMessage: "" };
    case ActionTypes.ERROR_LOADING_SERVERS:
      return {
        ...state,
        servers: [],
        errorMessage: action.payload.errorMessage,
      };
    case ActionTypes.ERROR_DELETING_SERVER:
      return {
        ...state,
        errorMessage: action.payload.errorMessage,
      };
    default:
      return state;
  }
}
