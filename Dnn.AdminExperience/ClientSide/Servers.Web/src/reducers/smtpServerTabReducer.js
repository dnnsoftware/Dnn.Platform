import { smtpServerTab as ActionTypes } from "../constants/actionTypes";
import validateFields from "../validation/validationSmtpServerTab";
import utils from "../utils";
import localization from "../localization";

export default function smtpServerTabReducer(
  state = {
    smtpServerInfo: {},
    errorMessage: "",
    infoMessage: "",
    errors: {},
  },
  action,
) {
  switch (action.type) {
    case ActionTypes.LOAD_SMTP_SERVER_TAB:
      return { ...state, smtpServerInfo: {}, isDirty: false, errorMessage: "" };
    case ActionTypes.LOADED_SMTP_SERVER_TAB:
      return {
        ...state,
        smtpServerInfo: action.payload.smtpServerInfo,
        errorMessage: "",
        errors: {},
      };
    case ActionTypes.LOAD_OAUTH_PROVIDERS:
      return {
        ...state,
        authProviders: {},
        errorMessage: "",
        providerChanged: false,
      };
    case ActionTypes.LOADED_OAUTH_PROVIDERS:
      return {
        ...state,
        authProviders: action.payload.authProviders,
        errorMessage: "",
        errors: {},
      };
    case ActionTypes.ERROR_LOADING_SMTP_SERVER_TAB:
      return {
        ...state,
        smtpServerInfo: {},
        isDirty: false,
        authProviders: {},
        errorMessage: action.payload.errorMessage,
      };
    case ActionTypes.CHANGE_SMTP_SERVER_MODE: {
      let errors = {};
      if (action.payload.smtpServeMode === "h") {
        if (utils.isHostUser()) {
          const smtpSettings = state.smtpServerInfo.host;
          errors = {
            ...validateFields(
              "smtpConnectionLimit",
              smtpSettings.smtpConnectionLimit,
            ),
            ...validateFields("smtpMaxIdleTime", smtpSettings.smtpMaxIdleTime),
            ...validateFields(
              "messageSchedulerBatchSize",
              smtpSettings.messageSchedulerBatchSize,
            ),
          };
        }
      } else {
        const smtpSettings = state.smtpServerInfo.site;
        errors = {
          ...validateFields(
            "smtpConnectionLimit",
            smtpSettings.smtpConnectionLimit,
          ),
          ...validateFields("smtpMaxIdleTime", smtpSettings.smtpMaxIdleTime),
        };
      }

      return {
        ...state,
        smtpServerInfo: {
          ...state.smtpServerInfo,
          smtpServerMode: action.payload.smtpServeMode,
        },
        isDirty: true,
        errors,
      };
    }
    case ActionTypes.CHANGE_SMTP_AUTHENTICATION: {
      if (state.smtpServerInfo.smtpServerMode === "h") {
        return {
          ...state,
          smtpServerInfo: {
            ...state.smtpServerInfo,
            host: {
              ...state.smtpServerInfo.host,
              smtpAuthentication: action.payload.smtpAuthentication,
            },
          },
          isDirty: true,
        };
      }

      return {
        ...state,
        smtpServerInfo: {
          ...state.smtpServerInfo,
          site: {
            ...state.smtpServerInfo.site,
            smtpAuthentication: action.payload.smtpAuthentication,
          },
        },
        isDirty: true,
      };
    }
    case ActionTypes.CHANGE_SMTP_CONFIGURATION_VALUE: {
      const field = action.payload.field;
      const value = action.payload.value;
      const passCheck = action.payload.passCheck;
      const isDirty = passCheck === true ? { ...state }.isDirty : true;
      const smtpServerInfo = {
        ...state.smtpServerInfo,
      };
      if (field === "messageSchedulerBatchSize") {
        smtpServerInfo.host = { ...state.smtpServerInfo.host };
        smtpServerInfo.host[field] = value;
      } else {
        if (state.smtpServerInfo.smtpServerMode === "h") {
          smtpServerInfo.host = { ...state.smtpServerInfo.host };
          smtpServerInfo.host[field] = value;
        } else {
          smtpServerInfo.site = { ...state.smtpServerInfo.site };
          smtpServerInfo.site[field] = value;
        }
      }
      return {
        ...state,
        smtpServerInfo,
        isDirty: isDirty,
        errors: {
          ...state.errors,
          ...validateFields(field, value),
        },
      };
    }
    case ActionTypes.UPDATE_SMTP_SERVER_SETTINGS:
      return { ...state, errorMessage: "", infoMessage: "" };
    case ActionTypes.UPDATED_SMTP_SERVER_SETTINGS: {
      const success = action.payload.success;
      const errors = action.payload.errors;
      const providerChanged = action.payload.providerChanged;
      const smtpServerInfo = { ...state.smtpServerInfo };

      if (success === false) {
        return {
          ...state,
          errorMessage: errors.join("<br />"),
          infoMessage: "",
        };
      } else {
        smtpServerInfo["isDirty"] = false;
        return {
          ...state,
          errorMessage: "",
          infoMessage: localization.get("SaveConfirmationMessage"),
          success: success,
          smtpServerInfo,
          isDirty: false,
          providerChanged: providerChanged,
        };
      }
    }
    case ActionTypes.ERROR_UPDATING_SMTP_SERVER_SETTINGS:
      return {
        ...state,
        errorMessage: action.payload.errorMessage,
        infoMessage: "",
      };

    case ActionTypes.SEND_TEST_EMAIL:
      return { ...state, errorMessage: "", infoMessage: "" };
    case ActionTypes.SENT_TEST_EMAIL:
      return {
        ...state,
        errorMessage: action.payload.success ? "" : action.payload.errorMessage,
        infoMessage: action.payload.success ? action.payload.infoMessage : "",
      };
    case ActionTypes.ERROR_SENDING_TEST_EMAIL:
      return {
        ...state,
        errorMessage: action.payload.errorMessage,
        infoMessage: "",
      };

    default:
      return state;
  }
}
