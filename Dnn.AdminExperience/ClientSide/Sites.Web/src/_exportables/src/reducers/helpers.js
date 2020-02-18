export function addPortalToList(portalsList, newPortal) {
    if (portalsList.some(portal => portal.PortalID === newPortal.PortalID)) {
        portalsList = portalsList.filter(portal => {
            return portal.PortalID !== newPortal.PortalID;
        });
    }
    if (!portalsList.some(portal => portal.PortalID === newPortal.PortalID)) {
        portalsList = [newPortal].concat(portalsList);
        portalsList = portalsList.sort(function (a, b) {
            let PortalIDA = a.PortalID;
            let PortalIDB = b.PortalID;
            if (PortalIDA < PortalIDB) //sort string ascending
                return -1;
            if (PortalIDA > PortalIDB)
                return 1;
            return 0;//default return value (no sorting)
        });
        return portalsList;
    }
}
export function addTemplateToList(templatesList, newTemplate) {
    if (templatesList.some(template => template.Value === newTemplate.Value)) {
        templatesList = templatesList.filter(template => {
            return template.Value !== newTemplate.Value;
        });
    }
    if (!templatesList.some(template => template.Value === newTemplate.Value)) {
        templatesList = [newTemplate].concat(templatesList);
        templatesList = templatesList.sort(function (a, b) {
            let NameA = a.Name;
            let NameB = b.Name;
            if (NameA < NameB) //sort string ascending
                return -1;
            if (NameA > NameB)
                return 1;
            return 0;//default return value (no sorting)
        });
        return templatesList;
    }
}

export function getFinalSwitchCase(switchCase, additionalCases) {
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