import {pagination as ActionTypes, portal as PortalActionTypes}  from "../constants/actionTypes";
import {PortalService} from "services";
const paginationActions = {
    loadTab(index) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_TAB_DATA,
                payload: {
                    index
                }
            });
        };
    },
    searchPortals(searchParameters) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEARCH_PORTALS,
                payload: {
                    filter: searchParameters.filter
                }
            });
            PortalService.getPortals(searchParameters, data => {
                dispatch({
                    type: PortalActionTypes.RETRIEVED_PORTALS,
                    payload: {
                        portals: data.Results,
                        totalCount: data.TotalResults
                    }
                });
            });
        };
    }
};

export default paginationActions;
