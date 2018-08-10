export function validationMapNewModule(newModule, getValidateRequired) {
    let _newModule = Object.assign({}, newModule);
    Object.keys(_newModule).forEach((key) => {
        let required = getValidateRequired(key);
        _newModule[key] = {
            value: _newModule[key],
            required,
            error: required ? !_newModule[key] : false
        };
    });
    return _newModule;
}

export function valueMapNewModule(newModule) {
    let _newModule = Object.assign({}, newModule);
    Object.keys(_newModule).forEach((key) => {
        _newModule[key] = _newModule[key].value;
    });
    return _newModule;
}