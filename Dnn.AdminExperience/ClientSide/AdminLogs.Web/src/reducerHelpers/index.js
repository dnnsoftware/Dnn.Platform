export function updateLogSettingList(logSettingList, logSettingDetail) {
    if (logSettingList.some(logSetting => logSetting.ID === logSettingDetail.ID)) {
        logSettingList = logSettingList.filter(logSetting => {
            return logSetting.ID !== logSettingDetail.ID;
        });
    }
    if (!logSettingList.some(logSetting => logSetting.ID === logSettingDetail.ID)) {
        logSettingList.push(logSettingDetail);
        logSettingList = logSettingList.sort(function (a, b) {
            let logTypeFriendlyNameA = a.LogTypeFriendlyName;
            let logTypeFriendlyNameB = b.LogTypeFriendlyName;
            if (logTypeFriendlyNameA < logTypeFriendlyNameB) //sort string asce`nding
                return -1;
            if (logTypeFriendlyNameA > logTypeFriendlyNameB)
                return 1;
            return 0;//default return value (no sorting)
        });
        return logSettingList;
    }
}

export function removeLogSetting(logSettingList, logSettingId) {
    if (logSettingList.some(logSetting => logSetting.ID === logSettingId)) {
        logSettingList = logSettingList.filter(logSetting => {
            return logSetting.ID !== logSettingId;
        });
    }
    return logSettingList;
}

export function createLogTypeOptions(actionLogTypeList) {
    let logTypeOptions = [];
    if (actionLogTypeList !== undefined) {
        logTypeOptions = actionLogTypeList.map((item) => {
            return { label: item.LogTypeFriendlyName, value: item.LogTypeKey };
        });
    }
    return logTypeOptions;
}

export function createPortalOptions(actionPortalList) {
    let portalOptions = [];
    if (actionPortalList !== undefined) {
        portalOptions = actionPortalList.map((item) => {
            return { label: item.PortalName, value: item.PortalID };
        });
    }
    return portalOptions;
}

export function createKeepMostRecentOptions(actionKeepMostRecentList) {
    let keepMostRecentOptions = [];
    if (actionKeepMostRecentList !== undefined) {
        keepMostRecentOptions = actionKeepMostRecentList.map((item) => {
            return { label: item.Key, value: item.Value };
        });
    }
    return keepMostRecentOptions;
}

export function createOccurrenceOptions(actionOccurrenceData) {
    let occurrenceOptions = { thresholdsOptions: [], notificationTimesOptions: [], notificationTimeTypesOptions: [] };
    occurrenceOptions.thresholdsOptions = actionOccurrenceData.thresholds.map((item) => {
        return { label: item.Key, value: item.Value };
    });
    occurrenceOptions.notificationTimesOptions = actionOccurrenceData.notificationTimes.map((item) => {
        return { label: item.Key, value: item.Value };
    });
    occurrenceOptions.notificationTimeTypesOptions = actionOccurrenceData.notificationTimeTypes.map((item) => {
        return { label: item.Key, value: item.Value };
    });
    return occurrenceOptions;
}
