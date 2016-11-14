import Localization from "./localization";

export default function validateFields(field, value) {
    const errors = {};

    if (field === "name") {
        if (!value) {
            errors[field] = Localization.get("NotEmptyNameError");
        }
        else {
            errors[field] = undefined;
        }
    }

    return errors;
}