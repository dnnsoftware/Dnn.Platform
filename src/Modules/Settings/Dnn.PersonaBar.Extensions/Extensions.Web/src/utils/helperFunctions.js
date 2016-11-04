function getValidateRequired(key) {
    switch (key) {
        case "friendlyName":
            return true;
        default:
            return false;
    }
}

function getTabMapping(key) {
    switch (key) {
        case "license":
            return 3;
        case "releaseNotes":
            return 4;
        default:
            return 0;
    }
}

export function validationMapExtensionBeingEdited(extensionBeingEdited) {
    let _extensionBeingEdited = Object.assign({}, extensionBeingEdited);
    Object.keys(_extensionBeingEdited).forEach((key) => {
        let validateRequired = getValidateRequired(key);
        let tabMapping = getTabMapping(key);
        _extensionBeingEdited[key] = {
            value: _extensionBeingEdited[key] && !_extensionBeingEdited[key].hasOwnProperty("value") ? _extensionBeingEdited[key] : _extensionBeingEdited[key] && _extensionBeingEdited[key].value,
            validateRequired,
            tabMapping,
            error: false
        };
    });
    return _extensionBeingEdited;
}

export function getVersionDropdownValues() {
    let values = [];
    for (let i = 0; i < 100; i++) {
        values.push({
            label: formatVersionNumber(i),
            value: i
        });
    }
    return values;
}
export function formatVersionNumber(n) {
    return n > 9 ? "" + n : "0" + n;
}