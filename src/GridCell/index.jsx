import React, {PropTypes, Component} from "react";
import "./style.less";

class GridCell extends Component {
    getStyle() {
        const {props} = this;
        return Object.assign({ width: (props.columnSize || 100) + (props.type ? props.type : "%") }, props.style);
    }
    render() {
        const {props} = this;
        return (
            <div className={"dnn-grid-cell " + props.className} style={this.getStyle() }>
                {props.children}
            </div>
        );
    }
}

GridCell.PropTypes = {
    children: PropTypes.node,
    columnSize: PropTypes.number,   
    type: PropTypes.string,
    style: PropTypes.object,
    className: PropTypes.string
};

GridCell.defaultProps = {
    className: ""
};

export default GridCell;