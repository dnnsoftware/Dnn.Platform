import localization from "./localization";

export default function validateFields(field, value) {
    const errors = {};

    if (field === "name") {
        if (!value) {
            errors[field] = localization.get("not_empty_name_error");
        }
        else {
            errors[field] = undefined;
        }
    }

    return errors;
}