export function formatVersionNumber(n) {
    return n > 9 ? "" + n : "0" + n;
}

function getValidateRequired(key) {
    switch (key) {
        case "friendlyName":
            return true;
        default:
            return false;
    }
}

export function validationMapExtensionBeingEdited(extensionBeingEdited) {
    let _extensionBeingEdited = Object.assign({}, extensionBeingEdited);
    Object.keys(_extensionBeingEdited).forEach((key) => {
        let validateRequired = getValidateRequired(key);
        _extensionBeingEdited[key] = {
            value: typeof _extensionBeingEdited[key] !== "object" ? _extensionBeingEdited[key] : _extensionBeingEdited[key].value,
            validateRequired,
            error: false
        };
    });
    return _extensionBeingEdited;
}

export function valueMapExtensionBeingEdited(extensionBeingEdited) {
    let _extensionBeingEdited = Object.assign({}, extensionBeingEdited);
    Object.keys(_extensionBeingEdited).forEach((key) => {
        _extensionBeingEdited[key] = _extensionBeingEdited[key].value;
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