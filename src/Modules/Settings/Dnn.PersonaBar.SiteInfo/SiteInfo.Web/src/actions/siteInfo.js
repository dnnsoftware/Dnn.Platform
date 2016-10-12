import {security as ActionTypes}  from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
import util from "../utils";

const siteInfoActions = {
    getPortalSettings() {
        return (dispatch) => {
            ApplicationService.getPortalSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITEINFO_PORTAL_SETTINGS,
                    data: {
                        settings: data.Results.settings,
                        timeZones: data.Results.timeZones,
						iconSets: data.action.iconSets
                    }
                });                
            });
        };
    },    
    updatePortalSettings(payload, callback) {
        return (dispatch) => {
            ApplicationService.updatePortalSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITEINFO_PORTAL_SETTINS,
                    data: {
                        
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    }
};

export default siteInfoActions;
