import React, {Component, PropTypes} from "react";

class MultiPersonaBarPage extends Component {
    constructor() {
        super();
    }
    render() {
        const {props} = this;
        return (
            <div className="personaBar-page multi" style={{left: props.left + "%", top: !props.repaintChildren ? props.top + "%" : 0, zIndex: props.zIndex}}>
                {props.children}
            </div>
        );
    }
}

MultiPersonaBarPage.PropTypes = {
    children: PropTypes.node,
    left: PropTypes.number,
    zIndex: PropTypes.number,
    repaint: PropTypes.bool
};

export default MultiPersonaBarPage;