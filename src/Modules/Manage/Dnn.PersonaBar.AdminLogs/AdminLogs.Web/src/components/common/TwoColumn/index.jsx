import React, {PropTypes, Component} from "react";
import "./style.less";

class TwoColumn extends Component {
    render() {
        const {props} = this;
        return (
            <div className="two-column">
                <div>
                    {props.columnOne}
                </div>
                <div className={props.rightColumnClassOverride ? props.rightColumnClass : ""}>
                    {props.columnTwo}
                </div>
            </div>
        );
    }
}

TwoColumn.PropTypes = {
    columnOne: PropTypes.node,
    columnTwo: PropTypes.node,
    rightColumnClassOverride: PropTypes.bool,
    rightColumnClass: PropTypes.string
};

export default TwoColumn;