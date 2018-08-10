import Localization from "./localization";

export default function validateFields(field, value) {
    const errors = {};
    const re = /^(|0|[1-9]\d*)$/;

    if (field === "name") {
        if (!value) {
            errors[field] = Localization.get("NotEmptyNameError");
        }
        else {
            errors[field] = undefined;
        }
    }
    else if (field === "cacheDuration" || field === "cacheMaxVaryByCount") {
        if (!re.test(value)) {
            errors[field] = Localization.get(field + ".ErrorMessage");
        }
        else {
            errors[field] = undefined;
        }
    }
    else if (field === "cacheProvider" && !value) {
        errors["cacheDuration"] = undefined;
        errors["cacheMaxVaryByCount"] = undefined;
    }

    return errors;
}