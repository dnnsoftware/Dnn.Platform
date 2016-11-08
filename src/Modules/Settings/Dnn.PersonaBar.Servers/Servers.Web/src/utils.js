let utilities = null;
let config = null;
let initialized = false;
let moduleName = null;

function init(options) {
    if (!options) {
        throw new Error("This method needs to have an options object as an input parameter");
    }
    if (!options.utilities) {
        throw new Error("This method needs to have an options.utilities object as an input parameter");
    }
    if (!options.config) {
        throw new Error("This method needs to have an options.config object as an input parameter");
    }
    utilities = options.utilities;  
    config = options.config; 
    initialized = true;
    moduleName = options.moduleName;   
}

function formatDateNoTime(date) {
    if (!initialized) {
        throw new Error("Utils have not been initialized");
    }
    const dateOptions = { year: "numeric", month: "numeric", day: "numeric" };
    return new Date(date).toLocaleDateString(config.culture, dateOptions);
}

function formatNumeric(value) {
    if (!initialized) {
        throw new Error("Utils have not been initialized");
    }
    return value.toLocaleString(config.culture);
}

function formatNumeric2Decimals(value) {
    return parseFloat(Math.round(value * 100) / 100).toFixed(2);
}

function notify(message) {
    if (!initialized) {
        throw new Error("Utils have not been initialized");
    }
    return utilities.notify(message);
}

function notifyError(message) {
    if (!initialized) {
        throw new Error("Utils have not been initialized");
    }
    return utilities.notifyError(message);
}

function getResx(moduleName, key) {
    if (!initialized) {
        throw new Error("Utils have not been initialized");
    }
    return utilities.getResx(moduleName, key);
}

function getModuleName() {
    if (!initialized) {
        throw new Error("Utils have not been initialized");
    }
    return moduleName;
}


const utils = {
    init,
    formatDateNoTime,
    formatNumeric,
    formatNumeric2Decimals,
    notify,
    notifyError,
    getResx,
    getModuleName
};

export default utils;
