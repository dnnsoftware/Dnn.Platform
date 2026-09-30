// Helpers for the "Permitted File Extension List" section of the More settings panel.
// Values must match AllowedExtensionsHelper on the server.
export const WhitelistOptions = Object.freeze({
    Default: 0,
    Custom: 1,
    OnlyImages: 2,
});

// Returns the distinct, lower cased extensions (without leading dot) of a comma separated list, sorted.
export function normalizeExtensions(list) {
    if (typeof list !== "string") {
        return [];
    }

    const extensions = list
        .split(",")
        .map((ext) => ext.trim().replace(/^\.+/, "").toLowerCase())
        .filter((ext) => ext.length > 0);
    return [...new Set(extensions)].sort();
}

// Whether two comma separated lists contain the same extensions, ignoring order, case, spaces and duplicates.
export function areEquivalent(first, second) {
    const a = normalizeExtensions(first);
    const b = normalizeExtensions(second);
    return a.length === b.length && a.every((ext, i) => ext === b[i]);
}

function isValidOption(option) {
    return Object.values(WhitelistOptions).indexOf(option) > -1;
}

// Gets the whitelist option for the given settings, preferring the option computed by the server.
export function getWhitelistOption(settings) {
    if (!settings) {
        return WhitelistOptions.Default;
    }

    if (isValidOption(settings.AllowedExtensionsWhitelistOption)) {
        return settings.AllowedExtensionsWhitelistOption;
    }

    if (areEquivalent(settings.AllowedExtensionsWhitelist, settings.HostAllowedExtensionsWhitelists)) {
        return WhitelistOptions.Default;
    }

    if (normalizeExtensions(settings.ImageExtensionsList).length > 0
        && areEquivalent(settings.AllowedExtensionsWhitelist, settings.ImageExtensionsList)) {
        return WhitelistOptions.OnlyImages;
    }

    return WhitelistOptions.Custom;
}

// Gets the list to start editing with when the custom option is used.
// A custom list is kept as is; otherwise the host's default list is used as a starting point
// (rather than the image list or an empty box).
export function getInitialCustomWhitelist(settings) {
    if (!settings) {
        return "";
    }

    return getWhitelistOption(settings) === WhitelistOptions.Custom
        ? settings.AllowedExtensionsWhitelist || ""
        : settings.HostAllowedExtensionsWhitelists || "";
}

// Gets the extensions list to show for the given option.
export function getWhitelistForOption(option, settings, customWhitelist) {
    switch (option) {
        case WhitelistOptions.Default:
            return settings.HostAllowedExtensionsWhitelists || "";
        case WhitelistOptions.OnlyImages:
            return settings.ImageExtensionsList || "";
        default:
            return typeof customWhitelist === "string"
                ? customWhitelist
                : settings.HostAllowedExtensionsWhitelists || "";
    }
}
