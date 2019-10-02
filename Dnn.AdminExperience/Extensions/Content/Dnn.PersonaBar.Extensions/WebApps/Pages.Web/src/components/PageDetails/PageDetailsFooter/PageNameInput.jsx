import React, {Component} from "react";
import PropTypes from "prop-types";
import { SingleLineInputWithError } from "@dnnsoftware/dnn-react-common";
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