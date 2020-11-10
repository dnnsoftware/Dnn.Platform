let localization = {};
let initialized = false;

function init(localizedResources) {
    localization = localizedResources;
    initialized = true;
}

function getString(key) {
    if (!initialized) {
        throw new Error("please call init method before use this method");
    }
    return localization[key] || "[" + key + "]";
}

export default {
    init,
    getString
};
