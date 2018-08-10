import React, {Component, PropTypes} from "react";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Localization from "../../../localization";

class PageNameInput extends Component {

    render() {
        const {props} = this;
        return <div> 
            <SingleLineInputWithError
                label={Localization.get("Name") + "*"}
                tooltipMessage={Localization.get("NameTooltip")}
                error={!!props.errors.name}
                errorMessage={props.errors.name}
                value={props.pageName}
                onChange={props.onChangePageName} />
            <div style={{ clear: "both" }}></div>
        </div>;
    }
}

PageNameInput.propTypes = {
    pageName: PropTypes.string.isRequired,
    errors: PropTypes.object.isRequired,
    onChangePageName: PropTypes.func.isRequired
};

export default PageNameInput;