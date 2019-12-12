// This is just used to mock the utils for the storybook story.
let utilities = null;
let config = null;
let moduleName = null;
let viewName = null;
let viewParams = null;
let initialized = false;
let settings = null;

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
    if (!options.moduleName) {
        throw new Error("This method needs to have an options.moduleName string as an input parameter");
    }
    utilities = options.utilities;
    config = options.config;
    moduleName = options.moduleName;
    viewName = options.viewName;
    viewParams = options.viewParams;
    initialized = true;
    settings = options.settings;
}

function load(options) {
    viewName = options.viewName;
    viewParams = options.viewParams;
    settings = options.settings;
}

function checkInit() {
    if (!initialized) {
        throw new Error("Utils have not been initialized");
    }
}

function formatDateNoTime(date) {
    checkInit();
    const dateOptions = { year: "numeric", month: "numeric", day: "numeric" };
    return new Date(date).toLocaleDateString(config.culture, dateOptions);
}

function formatNumeric(value) {
    checkInit();
    return value.toLocaleString(config.culture);
}

function formatNumeric2Decimals(value) {
    return parseFloat(Math.round(value * 100) / 100).toFixed(2);
}

function areEqualInvariantCase(a, b) {
    if (!a || !b) {
        return a === b;
    }
    return a.toLowerCase() === b.toLowerCase();
}

function notify(message, options) {
    checkInit();
    return utilities.notify(message, options);
}

function notifyError(message, options) {
    checkInit();
    return utilities.notifyError(message, options);
}

function confirm(message, confirmText, cancelText, confirmHandler, cancelHandler) {
    checkInit();
    return utilities.confirm(message, confirmText, cancelText, confirmHandler, cancelHandler);
}

function getServiceFramework() {
    checkInit();
    return utilities.sf;
}

function getUtilities() {
    checkInit();
    return utilities;
}

function getModuleName() {
    checkInit();
    return moduleName;
}

function getCurrentPageId() {
    checkInit();
    return parseInt(config.tabId);
}

function getViewName() {
    checkInit();
    return viewName;
}

function closePersonaBar(callback) {
    checkInit();
    utilities.closePersonaBar(callback);
}

function getViewParams() {
    checkInit();
    return viewParams;
}

function getResx(moduleName, key) {
    checkInit();
    return utilities.getResx(moduleName, key);
}

function getSiteRoot() {
    checkInit();
    return config.siteRoot;
}

function getPortalName() {
    checkInit();
    return settings && settings.portalName;
}

function getTemplateFolder() {
    checkInit();
    return settings.templateFolder;
}

function getIsSuperUser() {
    checkInit();
    return settings.isHost || settings.isAdmin;
}

function canSeePagesList() {
    checkInit();
    return settings.isHost || settings.isAdmin || settings.canSeePagesList;
}

function getCurrentPagePermissions() {
    checkInit();
    return settings.currentPagePermissions;
}

function getCurrentParentHasChildren() {
    checkInit();
    return settings.currentParentHasChildren;
}

function getCurrentPageName() {
    checkInit();
    return settings.currentPageName;
}

function getDefaultPageUrl() {
    checkInit();
    return config.siteRoot;
}
function getProductSKU() {
    checkInit();
    return settings.productSKU;
}
function isPlatform(){
    checkInit();
    return settings.productSKU.toLowerCase() === 'dnn';
}
function getIsAdminHostSystemPage() {
    checkInit();
    return settings.isAdminHostSystemPage;
}
function formatDate(dateValue, longformat) {
    if (!dateValue) {
        return "";
    }
    let date = new Date(dateValue);
    let yearValue = date.getFullYear();
    if (yearValue < 1900) {
        return "-";
    }

    return Moment(dateValue).locale(utilities.getCulture()).format(longformat === true ? "LLL" : "L");
}
function getUserMode(){
    return config.userMode;
}
const url = {
    appendQueryString: function(url, params){
        let urlParse = new UrlParse(url, true);
        let newParams = Object.assign({}, urlParse.query, params);
        urlParse.set('query', newParams);
        return urlParse.href;
    }
};
const utils = {
    init,
    load,
    formatDateNoTime,
    formatNumeric,
    formatNumeric2Decimals,
    notify,
    notifyError,
    confirm,
    getServiceFramework,
    getUtilities,
    getModuleName,
    getCurrentPageId,
    getViewName,
    closePersonaBar,
    getViewParams,
    getResx,
    getSiteRoot,
    areEqualInvariantCase,
    getPortalName,
    getTemplateFolder,
    getIsSuperUser,
    canSeePagesList,
    getCurrentPagePermissions,
    getCurrentParentHasChildren,
    getCurrentPageName,
    getDefaultPageUrl,
    getProductSKU,
    isPlatform,
    getIsAdminHostSystemPage,
    formatDate,
    getUserMode,
    url
};

export default utils;