import React, {Component, PropTypes} from "react";
import { Scrollbars } from "react-custom-scrollbars";

class PersonaBarPage extends Component {
    constructor() {
        super();
    }
    render() {
        const {props} = this;
        return (
            <div className={"personaBar-page" + (props.childPage ? " child" : "") } style={{ left: props.left + "%", top: props.top + "%" }}>
                <Scrollbars style={{ width: "100%", height: "100%"}}>
                    {props.children}
                </Scrollbars>
            </div>
        );
    }
}

PersonaBarPage.PropTypes = {
    children: PropTypes.node,
    left: PropTypes.number,
    childPage: PropTypes.bool
};

export default PersonaBarPage;